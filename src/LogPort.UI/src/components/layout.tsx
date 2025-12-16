import { type ReactNode } from 'react'
import '../styles/layout.css'
import * as Icons from '@mui/icons-material'
import { NavLink } from 'react-router-dom'
import Divider from './divider'

type LayoutProps = {
    children: ReactNode
}

export default function Layout({ children }: LayoutProps) {
    return (
        <div className="layout">
            <aside className="sidebar">
                {/* App / Logo area */}
                <div className="sidebar-header">
                    <img src='public/vite.svg' className="app-logo"></img>
                    <span className="app-name">LogPort</span>
                </div>

                <nav className="sidebar-nav">
                    <SidebarItem
                        icon={<Icons.DashboardOutlined />}
                        label="Dashboard"
                        href="/"
                    />
                    <Divider />
                    <SidebarItem
                        icon={<Icons.ReceiptLongOutlined />}
                        label="Log Explorer"
                        href="/logs"
                    />
                    <SidebarItem
                        icon={<Icons.StreamOutlined />}
                        label="Live Tailing"
                        href="/logs/tail"
                    />
                </nav>
            </aside>

            <main className="content">{children}</main>
        </div>
    )
}

type SidebarItemProps = {
    icon: ReactNode
    label: string
    href: string
}

function SidebarItem({ icon, label, href }: SidebarItemProps) {
    return (
        <NavLink
            to={href}
            className={({ isActive }) =>
                `sidebar-item${isActive ? ' active' : ''}`
            }
        >
            <span className="icon">{icon}</span>
            <span className="label">{label}</span>
        </NavLink>
    )
}
