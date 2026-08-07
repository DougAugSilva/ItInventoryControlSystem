import { useEffect, useState } from 'react';
import {
  Bar,
  BarChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';
import { apiJson } from '../services/api';
import { AVAILABILITY_STATUS, ITEM_CONDITION } from '../constants/items';
import './Dashboard.css';

export default function Dashboard() {
  const [status, setStatus] = useState('');
  const [condition, setCondition] = useState('');
  const [data, setData] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const params = new URLSearchParams();
    if (status !== '') params.set('status', status);
    if (condition !== '') params.set('condition', condition);

    setLoading(true);
    apiJson(`/api/dashboard/statistics?${params.toString()}`)
      .then(setData)
      .catch(() => setData([]))
      .finally(() => setLoading(false));
  }, [status, condition]);

  return (
    <div className="dashboard-page page-box">
      <div className="page-box-header">
        <h1>Dashboard</h1>
      </div>

      <div className="page-box-body">
        <div className="dashboard-filters">
          <label>
            Availability status
            <select value={status} onChange={(event) => setStatus(event.target.value)}>
              <option value="">All</option>
              {AVAILABILITY_STATUS.map((s) => (
                <option key={s.value} value={s.value}>
                  {s.label}
                </option>
              ))}
            </select>
          </label>

          <label>
            Item condition
            <select value={condition} onChange={(event) => setCondition(event.target.value)}>
              <option value="">All</option>
              {ITEM_CONDITION.map((e) => (
                <option key={e.value} value={e.value}>
                  {e.label}
                </option>
              ))}
            </select>
          </label>
        </div>

        <div className="dashboard-chart">
          {!loading && data.length === 0 && (
            <p className="dashboard-empty">No items found for the selected filters.</p>
          )}

          {data.length > 0 && (
            <ResponsiveContainer width="100%" height={420}>
              <BarChart data={data} margin={{ top: 16, right: 16, bottom: 60, left: 0 }}>
                <CartesianGrid strokeDasharray="3 3" stroke="#e0d8c4" />
                <XAxis
                  dataKey="typeName"
                  angle={-35}
                  textAnchor="end"
                  interval={0}
                  height={90}
                  tick={{ fontSize: 12, fill: '#2b2b2b' }}
                />
                <YAxis allowDecimals={false} tick={{ fontSize: 12, fill: '#2b2b2b' }} />
                <Tooltip />
                <Bar dataKey="count" fill="#1e90ff" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          )}
        </div>
      </div>
    </div>
  );
}
