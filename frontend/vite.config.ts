import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// Porta fixa (5180) -- o script start-cargoflow.ps1 espera essa porta
// especificamente antes de abrir o navegador.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5180,
    strictPort: true,
  },
})
