import { useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { apiJson, fileUrl } from '../services/api';
import { AVAILABILITY_STATUS, ITEM_CONDITION } from '../constants/items';
import SquareImage from './SquareImage';
import './SearchBar.css';

export default function SearchBar() {
  const [term, setTerm] = useState('');
  const [status, setStatus] = useState('');
  const [condition, setCondition] = useState('');
  const [results, setResults] = useState([]);
  const [open, setOpen] = useState(false);
  const [filtersOpen, setFiltersOpen] = useState(false);
  const containerRef = useRef(null);
  const navigate = useNavigate();

  useEffect(() => {
    function closeOnOutsideClick(event) {
      if (containerRef.current && !containerRef.current.contains(event.target)) {
        setOpen(false);
        setFiltersOpen(false);
      }
    }
    document.addEventListener('mousedown', closeOnOutsideClick);
    return () => document.removeEventListener('mousedown', closeOnOutsideClick);
  }, []);

  useEffect(() => {
    if (!term && status === '' && condition === '') {
      setResults([]);
      return;
    }

    const params = new URLSearchParams();
    if (term) params.set('search', term);
    if (status !== '') params.set('status', status);
    if (condition !== '') params.set('condition', condition);

    const timeout = setTimeout(async () => {
      try {
        const data = await apiJson(`/api/items?${params.toString()}`);
        setResults(data.slice(0, 8));
        setOpen(true);
      } catch {
        setResults([]);
      }
    }, 300);

    return () => clearTimeout(timeout);
  }, [term, status, condition]);

  function openItem(id) {
    setOpen(false);
    setTerm('');
    navigate(`/?edit=${id}`);
  }

  return (
    <div className="search-bar" ref={containerRef}>
      <button
        type="button"
        className="search-bar-icon"
        aria-label="Search"
        onClick={() => results.length > 0 && setOpen(true)}
      >
        <SearchIcon />
      </button>

      <input
        type="search"
        placeholder="Search by asset number, model, or type..."
        value={term}
        onChange={(event) => setTerm(event.target.value)}
        onFocus={() => results.length > 0 && setOpen(true)}
      />

      <button
        type="button"
        className="search-bar-icon"
        aria-label="Filters"
        onClick={() => setFiltersOpen((v) => !v)}
      >
        <FilterIcon />
      </button>

      {filtersOpen && (
        <div className="search-filters">
          <select value={status} onChange={(event) => setStatus(event.target.value)}>
            <option value="">Availability</option>
            {AVAILABILITY_STATUS.map((s) => (
              <option key={s.value} value={s.value}>
                {s.label}
              </option>
            ))}
          </select>

          <select value={condition} onChange={(event) => setCondition(event.target.value)}>
            <option value="">Condition</option>
            {ITEM_CONDITION.map((e) => (
              <option key={e.value} value={e.value}>
                {e.label}
              </option>
            ))}
          </select>
        </div>
      )}

      {open && results.length > 0 && (
        <ul className="search-results">
          {results.map((item) => (
            <li key={item.id} onClick={() => openItem(item.id)}>
              <SquareImage src={fileUrl(item.photoUrl)} alt={item.modelBrand} size={36} />
              <div>
                <strong>{item.modelBrand}</strong>
                <span>{item.itemTypeName}</span>
              </div>
            </li>
          ))}
        </ul>
      )}

      {open && term && results.length === 0 && (
        <ul className="search-results">
          <li className="search-empty">No items found :(</li>
        </ul>
      )}
    </div>
  );
}

function SearchIcon() {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
      <circle cx="11" cy="11" r="7" />
      <path d="M21 21l-4.3-4.3" strokeLinecap="round" />
    </svg>
  );
}

function FilterIcon() {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
      <path d="M4 5h16l-6 7v6l-4 2v-8z" strokeLinejoin="round" strokeLinecap="round" />
    </svg>
  );
}
