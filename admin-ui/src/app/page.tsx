import styles from "./page.module.css";

export default function Home() {
  return (
    <div className={styles.page}>
      <main className={styles.main}>
        <p className={styles.eyebrow}>GinsengFood · IVR Order Confirmation</p>
        <h1>IVR Admin — MOCK mode</h1>
        <p className={styles.summary}>
          Foundation workspace only. Sales data, SIM control, customer calls,
          and authentication remain disconnected from real providers.
        </p>
        <dl className={styles.statusGrid}>
          <div>
            <dt>Execution</dt>
            <dd>MOCK</dd>
          </div>
          <div>
            <dt>Sales provider</dt>
            <dd>FAKE_TARGET_V1</dd>
          </div>
          <div>
            <dt>Real calls</dt>
            <dd>DISABLED</dd>
          </div>
        </dl>
        <p className={styles.notice}>
          No order or customer data is available in this bootstrap.
        </p>
      </main>
    </div>
  );
}
