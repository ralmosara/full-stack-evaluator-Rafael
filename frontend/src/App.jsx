import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import Login from './pages/Login';
import Register from './pages/Register';
import Tasks from './Tasks';
import './App.css';

function AppRoutes() {
  const { isAuthenticated, email, logout } = useAuth();

  return (
    <div className="app">
      {isAuthenticated && (
        <header className="app-header">
          <h1>Task Manager</h1>
          <div className="user-info">
            <span>{email}</span>
            <button onClick={logout} className="btn btn-logout">Logout</button>
          </div>
        </header>
      )}
      <main>
        <Routes>
          <Route path="/login" element={isAuthenticated ? <Navigate to="/" /> : <Login />} />
          <Route path="/register" element={isAuthenticated ? <Navigate to="/" /> : <Register />} />
          <Route path="/" element={
            <ProtectedRoute>
              <Tasks />
            </ProtectedRoute>
          } />
          <Route path="*" element={<Navigate to="/" />} />
        </Routes>
      </main>
    </div>
  );
}

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <AppRoutes />
      </AuthProvider>
    </BrowserRouter>
  );
}

export default App;
