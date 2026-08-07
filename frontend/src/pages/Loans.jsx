import { useEffect, useState } from 'react';
import { apiJson, fileUrl } from '../services/api';
import SquareImage from '../components/SquareImage';
import './Loans.css';

function formatDate(iso) {
  if (!iso) return '—';
  return new Date(iso).toLocaleDateString('en-US');
}

export default function Loans() {
  const [loans, setLoans] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    apiJson('/api/loans')
      .then(setLoans)
      .catch(() => setError('Could not load loans.'))
      .finally(() => setLoading(false));
  }, []);

  return (
    <div className="loans-page page-box">
      <div className="page-box-header">
        <h1>Loans</h1>
      </div>

      <div className="page-box-body">
        {error && <p className="loans-error">{error}</p>}
        {!loading && loans.length === 0 && !error && (
          <p className="loans-empty">No loans recorded yet.</p>
        )}

        {loans.length > 0 && (
          <table className="loans-table">
            <thead>
              <tr>
                <th>Item</th>
                <th>Date</th>
                <th>Checked out by</th>
                <th>Loaned to</th>
                <th>Status</th>
                <th>Return date</th>
              </tr>
            </thead>
            <tbody>
              {loans.map((loan) => (
                <tr key={loan.id}>
                  <td className="loans-item">
                    <SquareImage src={fileUrl(loan.itemPhotoUrl)} alt={loan.itemModelBrand} size={40} />
                    <div>
                      <strong>{loan.itemModelBrand}</strong>
                      <span className="loans-type">{loan.itemTypeName}</span>
                    </div>
                  </td>
                  <td>{formatDate(loan.checkoutDate)}</td>
                  <td>{loan.technician}</td>
                  <td>{loan.borrowedBy}</td>
                  <td>
                    {loan.returnedAt ? (
                      <span className="loans-badge returned">
                        Returned on {formatDate(loan.returnedAt)}
                      </span>
                    ) : (
                      <span className="loans-badge loaned">Loaned</span>
                    )}
                  </td>
                  <td>{loan.returnDueDate ? formatDate(loan.returnDueDate) : '—'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
