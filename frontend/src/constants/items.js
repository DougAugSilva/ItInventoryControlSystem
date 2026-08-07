// Mirrors the backend's AvailabilityStatus and ItemCondition enums (Models/*.cs).
export const AVAILABILITY_STATUS = [
  { value: 0, label: 'Available' },
  { value: 1, label: 'Loaned' },
  { value: 2, label: 'Unavailable' },
];

export const ITEM_CONDITION = [
  { value: 0, label: 'New' },
  { value: 1, label: 'Used' },
  { value: 2, label: 'Defective' },
  { value: 3, label: 'Broken' },
];

export function availabilityLabel(value) {
  return AVAILABILITY_STATUS.find((s) => s.value === value)?.label ?? '—';
}

export function conditionLabel(value) {
  return ITEM_CONDITION.find((e) => e.value === value)?.label ?? '—';
}
