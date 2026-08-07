import { useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { apiForm, apiJson, fileUrl } from '../services/api';
import { AVAILABILITY_STATUS, ITEM_CONDITION } from '../constants/items';
import SquareImage from '../components/SquareImage';
import './Home.css';

const EMPTY_FORM = {
  assetNumber: '',
  itemTypeId: '',
  modelBrand: '',
  additionalInfo: '',
  availabilityStatus: 0,
  condition: 0,
  returnDueDate: '',
  technician: '',
  borrowedBy: '',
};

export default function Home() {
  const [searchParams, setSearchParams] = useSearchParams();
  const itemId = searchParams.get('edit');

  const [itemTypes, setItemTypes] = useState([]);
  const [form, setForm] = useState(EMPTY_FORM);
  const [photoFile, setPhotoFile] = useState(null);
  const [photoPreview, setPhotoPreview] = useState(null);
  const [message, setMessage] = useState(null);
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    apiJson('/api/item-types').then(setItemTypes).catch(() => setItemTypes([]));
  }, []);

  useEffect(() => {
    setMessage(null);
    setError('');
    setPhotoFile(null);

    if (!itemId) {
      setForm(EMPTY_FORM);
      setPhotoPreview(null);
      return;
    }

    apiJson(`/api/items/${itemId}`)
      .then((item) => {
        setForm({
          assetNumber: item.assetNumber ?? '',
          itemTypeId: item.itemTypeId,
          modelBrand: item.modelBrand,
          additionalInfo: item.additionalInfo ?? '',
          availabilityStatus: item.availabilityStatus,
          condition: item.condition,
          returnDueDate: item.activeLoan?.returnDueDate?.slice(0, 10) ?? '',
          technician: item.activeLoan?.technician ?? '',
          borrowedBy: item.activeLoan?.borrowedBy ?? '',
        });
        setPhotoPreview(fileUrl(item.photoUrl));
      })
      .catch(() => setError('Could not load the selected item.'));
  }, [itemId]);

  function updateField(field, value) {
    setForm((current) => ({ ...current, [field]: value }));
  }

  function handlePhoto(event) {
    const file = event.target.files?.[0];
    if (!file) return;

    if (!['image/jpeg', 'image/png'].includes(file.type)) {
      setError('The photo must be a JPG or PNG file.');
      return;
    }
    if (file.size > 10 * 1024 * 1024) {
      setError('The photo must be at most 10MB.');
      return;
    }

    setError('');
    setPhotoFile(file);
    setPhotoPreview(URL.createObjectURL(file));
  }

  async function handleSubmit(event) {
    event.preventDefault();
    setError('');
    setMessage(null);

    if (!form.itemTypeId || !form.modelBrand.trim()) {
      setError('Fill in at least the item type and the model/brand.');
      return;
    }
    if (form.availabilityStatus === 1 && (!form.technician.trim() || !form.borrowedBy.trim())) {
      setError('For loaned items, inform the responsible technician and who it was loaned to.');
      return;
    }

    const data = new FormData();
    if (form.assetNumber !== '') data.append('AssetNumber', form.assetNumber);
    data.append('ItemTypeId', form.itemTypeId);
    data.append('ModelBrand', form.modelBrand);
    if (form.additionalInfo) data.append('AdditionalInfo', form.additionalInfo);
    data.append('AvailabilityStatus', form.availabilityStatus);
    data.append('Condition', form.condition);
    if (form.availabilityStatus === 1) {
      if (form.returnDueDate) data.append('ReturnDueDate', form.returnDueDate);
      data.append('Technician', form.technician);
      data.append('BorrowedBy', form.borrowedBy);
    }
    if (photoFile) data.append('Photo', photoFile);

    setSaving(true);
    try {
      const saved = itemId
        ? await apiForm(`/api/items/${itemId}`, { method: 'PUT', formData: data })
        : await apiForm('/api/items', { method: 'POST', formData: data });

      setMessage(itemId ? 'Item updated successfully.' : 'Item registered successfully.');
      setSearchParams(itemId ? { edit: itemId } : {});
      if (!itemId) {
        setForm(EMPTY_FORM);
        setPhotoFile(null);
        setPhotoPreview(null);
      } else {
        setPhotoPreview(fileUrl(saved.photoUrl));
      }
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  }

  function newRegistration() {
    setSearchParams({});
  }

  return (
    <div className="home-page page-box">
      <div className="page-box-header">
        <h1>Register or Edit Item</h1>
        {itemId && (
          <button type="button" className="home-new" onClick={newRegistration}>
            + New item
          </button>
        )}
      </div>

      <form className="home-form page-box-body" onSubmit={handleSubmit}>
        <div className="home-form-photo">
          <SquareImage src={photoPreview} alt={form.modelBrand} size={140} />
          <label className="home-photo-input">
            Choose photo (JPG or PNG, up to 10MB)
            <input type="file" accept="image/jpeg,image/png" onChange={handlePhoto} />
          </label>
        </div>

        <div className="home-form-fields">
          <label>
            Asset number
            <input
              type="number"
              step="any"
              value={form.assetNumber}
              onChange={(event) => updateField('assetNumber', event.target.value)}
            />
          </label>

          <label>
            Item type
            <select
              value={form.itemTypeId}
              onChange={(event) => updateField('itemTypeId', event.target.value)}
              required
            >
              <option value="">Select...</option>
              {itemTypes.map((type) => (
                <option key={type.id} value={type.id}>
                  {type.name}
                </option>
              ))}
            </select>
          </label>

          <label>
            Model and brand
            <input
              type="text"
              maxLength={100}
              value={form.modelBrand}
              onChange={(event) => updateField('modelBrand', event.target.value)}
              required
            />
          </label>

          <label>
            Additional information
            <textarea
              maxLength={200}
              rows={3}
              value={form.additionalInfo}
              onChange={(event) => updateField('additionalInfo', event.target.value)}
            />
          </label>

          <fieldset>
            <legend>Availability status</legend>
            {AVAILABILITY_STATUS.map((s) => (
              <label key={s.value} className="home-radio">
                <input
                  type="radio"
                  name="availabilityStatus"
                  checked={form.availabilityStatus === s.value}
                  onChange={() => updateField('availabilityStatus', s.value)}
                />
                {s.label}
              </label>
            ))}
          </fieldset>

          {form.availabilityStatus === 1 && (
            <div className="home-loan">
              <label>
                Due back by
                <input
                  type="date"
                  min={new Date().toISOString().slice(0, 10)}
                  value={form.returnDueDate}
                  onChange={(event) => updateField('returnDueDate', event.target.value)}
                />
              </label>
              <label>
                Responsible technician
                <input
                  type="text"
                  value={form.technician}
                  onChange={(event) => updateField('technician', event.target.value)}
                  required
                />
              </label>
              <label>
                Loaned to whom
                <input
                  type="text"
                  value={form.borrowedBy}
                  onChange={(event) => updateField('borrowedBy', event.target.value)}
                  required
                />
              </label>
            </div>
          )}

          <fieldset>
            <legend>Item condition</legend>
            {ITEM_CONDITION.map((e) => (
              <label key={e.value} className="home-radio">
                <input
                  type="radio"
                  name="condition"
                  checked={form.condition === e.value}
                  onChange={() => updateField('condition', e.value)}
                />
                {e.label}
              </label>
            ))}
          </fieldset>

          {error && <p className="home-error">{error}</p>}
          {message && <p className="home-success">{message}</p>}

          <button type="submit" disabled={saving}>
            {saving ? 'Saving...' : itemId ? 'Save changes' : 'Register item'}
          </button>
        </div>
      </form>
    </div>
  );
}
