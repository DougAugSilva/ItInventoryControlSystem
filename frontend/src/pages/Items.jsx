import { useEffect, useState } from 'react';
import { apiJson, fileUrl } from '../services/api';
import { availabilityLabel, conditionLabel } from '../constants/items';
import SquareImage from '../components/SquareImage';
import './Items.css';

function formatDate(iso) {
  if (!iso) return '—';
  return new Date(iso).toLocaleString('en-US');
}

export default function Items() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    apiJson('/api/items?sort=recent')
      .then(setItems)
      .catch(() => setError('Could not load items.'))
      .finally(() => setLoading(false));
  }, []);

  return (
    <div className="items-page page-box">
      <div className="page-box-header">
        <h1>Items</h1>
      </div>

      <div className="page-box-body">
        {error && <p className="items-error">{error}</p>}
        {!loading && items.length === 0 && !error && (
          <p className="items-empty">No items registered yet.</p>
        )}

        {items.length > 0 && (
          <table className="items-table">
            <thead>
              <tr>
                <th>Item</th>
                <th>Status</th>
                <th>Condition</th>
                <th>Date</th>
                <th>Registered by</th>
              </tr>
            </thead>
            <tbody>
              {items.map((item) => (
                <tr key={item.id}>
                  <td className="items-item">
                    <SquareImage src={fileUrl(item.photoUrl)} alt={item.modelBrand} size={40} />
                    <div>
                      <strong>{item.modelBrand}</strong>
                      <span className="items-type">{item.itemTypeName}</span>
                    </div>
                  </td>
                  <td>{availabilityLabel(item.availabilityStatus)}</td>
                  <td>{conditionLabel(item.condition)}</td>
                  <td>{formatDate(item.createdAt)}</td>
                  <td>{item.createdByName}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
