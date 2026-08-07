import { useState } from 'react';
import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { fileUrl } from '../services/api';
import SearchBar from './SearchBar';
import './Layout.css';

const TABS = [
  { path: '/', label: 'Register', end: true },
  { path: '/dashboard', label: 'Dashboard' },
  { path: '/loans', label: 'Loans' },
  { path: '/items', label: 'Items' },
];

export default function Layout() {
  const { user, logout } = useAuth();
  const [menuOpen, setMenuOpen] = useState(false);
  const navigate = useNavigate();

  const tabs = user?.isAdmin
    ? [...TABS, { path: '/users', label: 'User Management' }]
    : TABS;

  return (
    <div className="layout">
      <header className="layout-header">
        <img
          className="layout-logo"
          src={fileUrl('/img/desing/logo_2.png')}
          alt="BestTechTI"
        />

        <nav className="layout-tabs">
          {tabs.map((tab) => (
            <NavLink key={tab.path} to={tab.path} end={tab.end}>
              {tab.label}
            </NavLink>
          ))}
        </nav>

        <SearchBar />

        <button type="button" className="layout-icon-button" onClick={() => navigate('/')}>
          <HomeIcon />
          Home
        </button>

        <div className="layout-user">
          <button type="button" className="layout-icon-button" onClick={() => setMenuOpen((v) => !v)}>
            <UserIcon />
            {user?.fullName}
          </button>

          {menuOpen && (
            <div className="layout-user-menu">
              <button type="button" onClick={logout}>
                Log out
              </button>
            </div>
          )}
        </div>
      </header>

      <main
        className="layout-content"
        style={{ backgroundImage: `url(${fileUrl('/img/desing/data_center_wallpaper.jpg')})` }}
      >
        <div className="layout-content-inner">
          <Outlet />
        </div>
      </main>
    </div>
  );
}

function HomeIcon() {
  return (
    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
      <path d="M3 11l9-8 9 8" strokeLinecap="round" strokeLinejoin="round" />
      <path d="M5 10v10h14V10" strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  );
}

function UserIcon() {
  return (
    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
      <circle cx="12" cy="8" r="4" />
      <path d="M4 21c0-4 3.5-7 8-7s8 3 8 7" strokeLinecap="round" />
    </svg>
  );
}
