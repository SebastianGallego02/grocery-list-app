import styles from './Skeleton.module.scss'

export interface SkeletonListProps {
  /** Announced to screen readers; the placeholder rows themselves are hidden from them. */
  label?: string
  rows?: number
}

/** Placeholder rows shaped like the content that is loading, so the layout does not jump. */
export function SkeletonList({ label = 'Loading…', rows = 4 }: SkeletonListProps) {
  return (
    <div role="status" className={styles.list}>
      <span className="visually-hidden">{label}</span>
      {Array.from({ length: rows }, (_, index) => (
        <div key={index} className={styles.row} aria-hidden="true">
          <span className={`${styles.block} ${styles.toggle}`} />
          <span className={`${styles.block} ${styles.text}`} />
          <span className={`${styles.block} ${styles.action}`} />
        </div>
      ))}
    </div>
  )
}
