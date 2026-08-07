// Crops any image ratio into a 1:1 square using CSS only (object-fit: cover),
// with no server-side image processing needed.
export default function SquareImage({ src, alt, size = 96 }) {
  return (
    <div
      style={{
        width: size,
        height: size,
        borderRadius: 'var(--radius)',
        overflow: 'hidden',
        background: 'var(--color-bg-soft)',
        flexShrink: 0,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
      }}
    >
      {src ? (
        <img
          src={src}
          alt={alt}
          style={{ width: '100%', height: '100%', objectFit: 'cover' }}
        />
      ) : (
        <span style={{ fontSize: '0.7rem', color: 'var(--color-text-soft)' }}>no photo</span>
      )}
    </div>
  );
}
