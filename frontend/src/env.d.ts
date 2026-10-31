/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** Base URL of the API. Empty means same origin (the dev server proxies /api). */
  readonly VITE_API_BASE_URL?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
