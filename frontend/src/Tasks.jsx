import { useEffect, useState } from 'react';
import api from './api/axios';

const PRIORITY_LABELS = ['', 'Lowest', 'Low', 'Medium', 'High', 'Highest'];
const PRIORITY_CLASSES = ['', 'priority-lowest', 'priority-low', 'priority-medium', 'priority-high', 'priority-highest'];

function Tasks() {
  const [tasks, setTasks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  // Create form state
  const [newTitle, setNewTitle] = useState('');
  const [newDescription, setNewDescription] = useState('');
  const [newPriority, setNewPriority] = useState(3);
  const [creating, setCreating] = useState(false);

  // Edit state
  const [editingId, setEditingId] = useState(null);
  const [editForm, setEditForm] = useState({});

  const fetchTasks = async () => {
    try {
      setLoading(true);
      const res = await api.get('/api/tasks');
      setTasks(res.data);
      setError('');
    } catch {
      setError('Failed to load tasks.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchTasks();
  }, []);

  const handleCreate = async (e) => {
    e.preventDefault();
    if (!newTitle.trim()) return;
    setCreating(true);
    try {
      await api.post('/api/tasks', {
        title: newTitle.trim(),
        description: newDescription.trim() || null,
        priority: newPriority,
      });
      setNewTitle('');
      setNewDescription('');
      setNewPriority(3);
      setError('');
      await fetchTasks();
    } catch {
      setError('Failed to create task.');
    } finally {
      setCreating(false);
    }
  };

  const handleToggleDone = async (task) => {
    try {
      await api.put(`/api/tasks/${task.id}`, {
        title: task.title,
        description: task.description,
        isDone: !task.isDone,
        priority: task.priority,
      });
      setError('');
      await fetchTasks();
    } catch {
      setError('Failed to update task.');
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Delete this task?')) return;
    try {
      await api.delete(`/api/tasks/${id}`);
      setError('');
      await fetchTasks();
    } catch {
      setError('Failed to delete task.');
    }
  };

  const startEdit = (task) => {
    setEditingId(task.id);
    setEditForm({
      title: task.title,
      description: task.description || '',
      isDone: task.isDone,
      priority: task.priority,
    });
  };

  const cancelEdit = () => {
    setEditingId(null);
    setEditForm({});
  };

  const handleEditSave = async (id) => {
    if (!editForm.title.trim()) return;
    try {
      await api.put(`/api/tasks/${id}`, {
        ...editForm,
        title: editForm.title.trim(),
        description: editForm.description.trim() || null,
      });
      setEditingId(null);
      setError('');
      await fetchTasks();
    } catch {
      setError('Failed to update task.');
    }
  };

  const formatDate = (dateStr) => {
    if (!dateStr) return '';
    return new Date(dateStr).toLocaleDateString(undefined, {
      month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit',
    });
  };

  return (
    <div className="tasks-container">
      {error && <p className="error-message">{error}</p>}

      {/* Create Task Form */}
      <form onSubmit={handleCreate} className="create-form">
        <div className="form-row">
          <input
            type="text"
            placeholder="Task title *"
            value={newTitle}
            onChange={(e) => setNewTitle(e.target.value)}
            required
            maxLength={200}
            className="input-title"
          />
          <select
            value={newPriority}
            onChange={(e) => setNewPriority(Number(e.target.value))}
            className="input-priority"
          >
            {[1, 2, 3, 4, 5].map((p) => (
              <option key={p} value={p}>{PRIORITY_LABELS[p]}</option>
            ))}
          </select>
          <button type="submit" disabled={creating} className="btn btn-primary">
            {creating ? 'Adding...' : 'Add Task'}
          </button>
        </div>
        <input
          type="text"
          placeholder="Description (optional)"
          value={newDescription}
          onChange={(e) => setNewDescription(e.target.value)}
          maxLength={1000}
          className="input-description"
        />
      </form>

      {/* Task List */}
      {loading ? (
        <p className="loading">Loading tasks...</p>
      ) : tasks.length === 0 ? (
        <p className="empty-state">No tasks yet. Create one above!</p>
      ) : (
        <ul className="task-list">
          {tasks.map((task) => (
            <li key={task.id} className={`task-item ${task.isDone ? 'done' : ''}`}>
              {editingId === task.id ? (
                <div className="edit-form">
                  <div className="form-row">
                    <input
                      type="text"
                      value={editForm.title}
                      onChange={(e) => setEditForm({ ...editForm, title: e.target.value })}
                      className="input-title"
                      maxLength={200}
                    />
                    <select
                      value={editForm.priority}
                      onChange={(e) => setEditForm({ ...editForm, priority: Number(e.target.value) })}
                      className="input-priority"
                    >
                      {[1, 2, 3, 4, 5].map((p) => (
                        <option key={p} value={p}>{PRIORITY_LABELS[p]}</option>
                      ))}
                    </select>
                  </div>
                  <input
                    type="text"
                    value={editForm.description}
                    onChange={(e) => setEditForm({ ...editForm, description: e.target.value })}
                    placeholder="Description (optional)"
                    className="input-description"
                    maxLength={1000}
                  />
                  <div className="edit-actions">
                    <button onClick={() => handleEditSave(task.id)} className="btn btn-save">Save</button>
                    <button onClick={cancelEdit} className="btn btn-cancel">Cancel</button>
                  </div>
                </div>
              ) : (
                <div className="task-content">
                  <div className="task-left">
                    <input
                      type="checkbox"
                      checked={task.isDone}
                      onChange={() => handleToggleDone(task)}
                      className="task-checkbox"
                    />
                    <div className="task-text">
                      <span className="task-title">{task.title}</span>
                      {task.description && (
                        <span className="task-description">{task.description}</span>
                      )}
                      <span className="task-meta">
                        {formatDate(task.createdAt)}
                        {task.updatedAt && ` (edited ${formatDate(task.updatedAt)})`}
                      </span>
                    </div>
                  </div>
                  <div className="task-right">
                    <span className={`priority-badge ${PRIORITY_CLASSES[task.priority]}`}>
                      {PRIORITY_LABELS[task.priority]}
                    </span>
                    <button onClick={() => startEdit(task)} className="btn btn-edit">Edit</button>
                    <button onClick={() => handleDelete(task.id)} className="btn btn-delete">Delete</button>
                  </div>
                </div>
              )}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

export default Tasks;
