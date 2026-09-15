import { useEffect, useState, type ComponentType } from 'react'
import { NavLink, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import {
  AlertTriangleIcon,
  BuildingIcon,
  ClipboardIcon,
  CollapseLeftIcon,
  CollapseRightIcon,
  DashboardIcon,
  DollarIcon,
  DropletIcon,
  FileTextIcon,
  type IconProps,
  LogoutIcon,
  MenuIcon,
  PackageIcon,
  RouteIcon,
  ShieldIcon,
  ToolIcon,
  TruckIcon,
  UserIcon,
} from './icons'

const SIDEBAR_COLLAPSED_KEY = 'cargoflow_sidebar_collapsed'

interface NavItem {
  to: string
  label: string
  icon: ComponentType<IconProps>
  end?: boolean
}

interface NavGroup {
  title: string | null
  items: NavItem[]
}

const NAV_GROUPS: NavGroup[] = [
  { title: null, items: [{ to: '/', label: 'Dashboard', icon: DashboardIcon, end: true }] },
  {
    title: 'Cadastros',
    items: [
      { to: '/empresas', label: 'Empresas', icon: BuildingIcon },
      { to: '/veiculos', label: 'Veículos', icon: TruckIcon },
      { to: '/motoristas', label: 'Motoristas', icon: UserIcon },
      { to: '/documentos', label: 'Documentos', icon: FileTextIcon },
    ],
  },
  {
    title: 'Operação',
    items: [
      { to: '/cotacoes', label: 'Cotações', icon: ClipboardIcon },
      { to: '/ordens-transporte', label: 'Ordens de Transporte', icon: PackageIcon },
      { to: '/viagens', label: 'Viagens', icon: RouteIcon },
      { to: '/ocorrencias', label: 'Ocorrências', icon: AlertTriangleIcon },
    ],
  },
  {
    title: 'Gestão',
    items: [
      { to: '/abastecimentos', label: 'Abastecimentos', icon: DropletIcon },
      { to: '/manutencao', label: 'Manutenção', icon: ToolIcon },
    ],
  },
]

export default function Layout() {
  const { user, logout } = useAuth()
  const location = useLocation()

  const [collapsed, setCollapsed] = useState(() => {
    try {
      return localStorage.getItem(SIDEBAR_COLLAPSED_KEY) === '1'
    } catch {
      return false
    }
  })
  const [mobileOpen, setMobileOpen] = useState(false)

  useEffect(() => {
    try {
      localStorage.setItem(SIDEBAR_COLLAPSED_KEY, collapsed ? '1' : '0')
    } catch {
      // localStorage indisponível (modo privado etc) -- só não persiste, sem quebrar nada
    }
  }, [collapsed])

  // Fecha o drawer mobile sozinho ao trocar de rota -- sem isso o usuário
  // navega e o menu continua aberto por cima do conteúdo novo.
  useEffect(() => {
    setMobileOpen(false)
  }, [location.pathname])

  const financeVisible = user?.role === 'Administrador' || user?.role === 'Financeiro'
  const auditVisible = user?.role === 'Administrador'

  const groups: NavGroup[] = [
    ...NAV_GROUPS,
    ...(financeVisible || auditVisible
      ? [
          {
            title: 'Sistema',
            items: [
              ...(financeVisible ? [{ to: '/financeiro', label: 'Financeiro', icon: DollarIcon }] : []),
              ...(auditVisible ? [{ to: '/auditoria', label: 'Auditoria', icon: ShieldIcon }] : []),
            ],
          },
        ]
      : []),
  ]

  const initials = (user?.fullName ?? '?')
    .split(' ')
    .map((p) => p[0])
    .slice(0, 2)
    .join('')
    .toUpperCase()

  return (
    <div className={`app-layout ${collapsed ? 'sidebar-collapsed' : ''}`}>
      <button className="sidebar-mobile-toggle" onClick={() => setMobileOpen(true)} aria-label="Abrir menu">
        <MenuIcon size={20} />
      </button>

      {mobileOpen && <div className="sidebar-backdrop" onClick={() => setMobileOpen(false)} />}

      <aside className={`sidebar ${mobileOpen ? 'mobile-open' : ''}`}>
        <div className="sidebar-header">
          <span className="sidebar-brand">{collapsed ? <TruckIcon size={22} /> : 'CargoFlow'}</span>
          <button className="sidebar-toggle" onClick={() => setCollapsed((c) => !c)} title={collapsed ? 'Expandir menu' : 'Recolher menu'}>
            {collapsed ? <CollapseRightIcon size={14} /> : <CollapseLeftIcon size={14} />}
          </button>
        </div>

        <nav className="sidebar-nav">
          {groups.map((group, i) => (
            <div className="sidebar-group" key={group.title ?? `group-${i}`}>
              {group.title && !collapsed && <span className="sidebar-group-title">{group.title}</span>}
              {group.items.map((item) => {
                const ItemIcon = item.icon
                return (
                  <NavLink
                    key={item.to}
                    to={item.to}
                    end={item.end}
                    className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
                    title={collapsed ? item.label : undefined}
                  >
                    <span className="sidebar-link-icon">
                      <ItemIcon size={18} />
                    </span>
                    <span className="sidebar-link-label">{item.label}</span>
                  </NavLink>
                )
              })}
            </div>
          ))}
        </nav>

        <div className="sidebar-footer">
          <div className="sidebar-user" title={collapsed ? `${user?.fullName} (${user?.role})` : undefined}>
            <span className="sidebar-user-avatar">{initials}</span>
            {!collapsed && (
              <span className="sidebar-user-info">
                <span className="sidebar-user-name">{user?.fullName}</span>
                <span className="sidebar-user-role">{user?.role}</span>
              </span>
            )}
          </div>
          <button className="sidebar-logout" onClick={logout} title="Sair">
            <LogoutIcon size={16} />
            {!collapsed && <span>Sair</span>}
          </button>
        </div>
      </aside>

      <div className="app-shell">
        <Outlet />
      </div>
    </div>
  )
}
