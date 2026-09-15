import { Navigate, Route, Routes } from 'react-router-dom'
import RequireAuth from './components/RequireAuth'
import Layout from './components/Layout'
import LoginPage from './pages/LoginPage'
import DashboardPage from './pages/DashboardPage'
import CompaniesPage from './pages/CompaniesPage'
import VehiclesPage from './pages/VehiclesPage'
import DriversPage from './pages/DriversPage'
import DocumentsPage from './pages/DocumentsPage'
import FreightQuotesPage from './pages/FreightQuotesPage'
import FreightPricingRulesPage from './pages/FreightPricingRulesPage'
import TransportOrdersPage from './pages/TransportOrdersPage'
import TripsPage from './pages/TripsPage'
import OccurrencesPage from './pages/OccurrencesPage'
import FuelingsPage from './pages/FuelingsPage'
import MaintenancePage from './pages/MaintenancePage'
import FinancePage from './pages/FinancePage'
import AuditLogPage from './pages/AuditLogPage'

function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />

      <Route
        element={
          <RequireAuth>
            <Layout />
          </RequireAuth>
        }
      >
        <Route path="/" element={<DashboardPage />} />
        <Route path="/empresas" element={<CompaniesPage />} />
        <Route path="/veiculos" element={<VehiclesPage />} />
        <Route path="/motoristas" element={<DriversPage />} />
        <Route path="/documentos" element={<DocumentsPage />} />
        <Route path="/cotacoes" element={<FreightQuotesPage />} />
        <Route path="/tabela-precos" element={<FreightPricingRulesPage />} />
        <Route path="/ordens-transporte" element={<TransportOrdersPage />} />
        <Route path="/viagens" element={<TripsPage />} />
        <Route path="/ocorrencias" element={<OccurrencesPage />} />
        <Route path="/abastecimentos" element={<FuelingsPage />} />
        <Route path="/manutencao" element={<MaintenancePage />} />
        <Route path="/financeiro" element={<FinancePage />} />
        <Route path="/auditoria" element={<AuditLogPage />} />
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}

export default App
