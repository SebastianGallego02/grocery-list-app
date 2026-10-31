import styles from './ProgressSummary.module.scss'

interface ProgressSummaryProps {
  inBasket: number
  total: number
}

/** "X of Y in basket" with a bar; the text carries the information, the bar is decorative. */
export function ProgressSummary({ inBasket, total }: ProgressSummaryProps) {
  const percent = total === 0 ? 0 : Math.round((inBasket / total) * 100)
  const isDone = total > 0 && inBasket === total

  return (
    <div className={styles.summary}>
      <p className={styles.text}>
        <strong>{inBasket}</strong> of <strong>{total}</strong> in basket
        {isDone && <span className={styles.done}> · All done!</span>}
      </p>
      <div className={styles.track} aria-hidden="true">
        <div className={styles.fill} style={{ width: `${percent}%` }} />
      </div>
    </div>
  )
}
