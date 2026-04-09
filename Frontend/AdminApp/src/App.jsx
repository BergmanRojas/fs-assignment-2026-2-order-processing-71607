import { useEffect, useMemo, useState } from 'react'
import './App.css'

const API_BASE_URL = 'http://localhost:5258'
const ADMIN_STORAGE_KEY = 'adminIsLoggedIn'

const FILTER_OPTIONS = [
    'All',
    'Submitted',
    'Payment Approved',
    'Completed',
    'Failed',
]

function App() {
    const [isLoggedIn, setIsLoggedIn] = useState(false)
    const [isCheckingSession, setIsCheckingSession] = useState(true)

    const [email, setEmail] = useState('admin@demo.com')
    const [password, setPassword] = useState('')
    const [loginError, setLoginError] = useState('')

    const [orders, setOrders] = useState([])
    const [selectedOrder, setSelectedOrder] = useState(null)
    const [selectedFailedOrder, setSelectedFailedOrder] = useState(null)

    const [statusFilter, setStatusFilter] = useState('All')
    const [searchTerm, setSearchTerm] = useState('')
    const [failedSearchTerm, setFailedSearchTerm] = useState('')

    const [isLoading, setIsLoading] = useState(true)
    const [isRefreshing, setIsRefreshing] = useState(false)
    const [errorMessage, setErrorMessage] = useState('')
    const [activeView, setActiveView] = useState('dashboard')

    useEffect(() => {
        const savedLogin = localStorage.getItem(ADMIN_STORAGE_KEY) === 'true'
        setIsLoggedIn(savedLogin)
        setIsCheckingSession(false)
    }, [])

    useEffect(() => {
        if (!isLoggedIn) return

        loadOrders(true)

        const interval = setInterval(() => {
            loadOrders(false)
        }, 10000)

        return () => clearInterval(interval)
    }, [isLoggedIn])

    async function loadOrders(showLoadingState = false) {
        if (showLoadingState) {
            setIsLoading(true)
        } else {
            setIsRefreshing(true)
        }

        setErrorMessage('')

        try {
            const response = await fetch(`${API_BASE_URL}/api/orders`)

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`)
            }

            const data = await response.json()
            setOrders(data)

            if (data.length > 0) {
                setSelectedOrder((current) => {
                    if (!current) return data[0]
                    return data.find((order) => order.orderId === current.orderId) ?? data[0]
                })
            } else {
                setSelectedOrder(null)
            }
        } catch (error) {
            setErrorMessage(`Error loading orders: ${error.message}`)
        } finally {
            setIsLoading(false)
            setIsRefreshing(false)
        }
    }

    function handleLogin() {
        setLoginError('')

        const isValid =
            email.trim().toLowerCase() === 'admin@demo.com' &&
            password === 'admin123'

        if (!isValid) {
            setLoginError('Invalid email or password.')
            return
        }

        localStorage.setItem(ADMIN_STORAGE_KEY, 'true')
        setIsLoggedIn(true)
        setPassword('')
    }

    function handleLogout() {
        localStorage.removeItem(ADMIN_STORAGE_KEY)
        setIsLoggedIn(false)
        setPassword('')
        setLoginError('')
        setOrders([])
        setSelectedOrder(null)
        setSelectedFailedOrder(null)
        setStatusFilter('All')
        setSearchTerm('')
        setFailedSearchTerm('')
        setErrorMessage('')
        setActiveView('dashboard')
    }

    const filteredOrders = useMemo(() => {
        return orders.filter((order) => {
            const matchesStatus =
                statusFilter === 'All' ? true : order.status === statusFilter

            const matchesSearch =
                searchTerm.trim() === ''
                    ? true
                    : order.orderId.toLowerCase().includes(searchTerm.trim().toLowerCase())

            return matchesStatus && matchesSearch
        })
    }, [orders, statusFilter, searchTerm])

    const failedOrders = useMemo(() => {
        return orders.filter((order) => order.status === 'Failed')
    }, [orders])

    const filteredFailedOrders = useMemo(() => {
        return failedOrders.filter((order) => {
            if (failedSearchTerm.trim() === '') return true

            const term = failedSearchTerm.trim().toLowerCase()

            return (
                order.orderId.toLowerCase().includes(term) ||
                order.customerId.toLowerCase().includes(term)
            )
        })
    }, [failedOrders, failedSearchTerm])

    const summary = useMemo(() => {
        return {
            total: orders.length,
            completed: orders.filter((order) => order.status === 'Completed').length,
            failed: orders.filter((order) => order.status === 'Failed').length,
            pending: orders.filter(
                (order) => order.status !== 'Completed' && order.status !== 'Failed'
            ).length,
        }
    }, [orders])

    useEffect(() => {
        if (filteredFailedOrders.length > 0) {
            setSelectedFailedOrder((current) => {
                if (!current) return filteredFailedOrders[0]
                return (
                    filteredFailedOrders.find((order) => order.orderId === current.orderId) ??
                    filteredFailedOrders[0]
                )
            })
        } else {
            setSelectedFailedOrder(null)
        }
    }, [filteredFailedOrders])

    function formatDate(value) {
        if (!value) return '-'

        const date = new Date(value)
        if (Number.isNaN(date.getTime())) return '-'

        return date.toLocaleString('en-IE', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit',
        })
    }

    function formatShortId(value) {
        if (!value) return '-'
        return value.length > 18 ? `${value.slice(0, 8)}...${value.slice(-6)}` : value
    }

    function getStatusClass(status) {
        const normalized = (status ?? '').toLowerCase()

        if (normalized === 'completed') return 'completed'
        if (normalized === 'failed') return 'failed'
        if (normalized.includes('payment')) return 'payment'
        if (normalized.includes('inventory')) return 'inventory'
        if (normalized.includes('shipping')) return 'shipping'
        if (normalized === 'submitted') return 'submitted'
        return 'default'
    }

    function renderOrderDetails(order) {
        if (!order) {
            return <div className="empty-state">Select an order to view details.</div>
        }

        return (
            <div className="details-grid">
                <div className="detail-card">
                    <span>Order ID</span>
                    <strong>{order.orderId}</strong>
                </div>
                <div className="detail-card">
                    <span>Customer ID</span>
                    <strong>{order.customerId}</strong>
                </div>
                <div className="detail-card">
                    <span>Status</span>
                    <strong>
            <span className={`status-badge ${getStatusClass(order.status)}`}>
              {order.status}
            </span>
                    </strong>
                </div>
                <div className="detail-card">
                    <span>Created</span>
                    <strong>{formatDate(order.createdAt)}</strong>
                </div>
                <div className="detail-card">
                    <span>Inventory Result</span>
                    <strong>{formatDate(order.inventoryConfirmedAt)}</strong>
                </div>
                <div className="detail-card">
                    <span>Payment Status</span>
                    <strong>{formatDate(order.paymentApprovedAt)}</strong>
                </div>
                <div className="detail-card">
                    <span>Shipping Status</span>
                    <strong>{formatDate(order.shippingCreatedAt)}</strong>
                </div>
                <div className="detail-card wide shipment-highlight">
                    <span>Shipment Reference</span>
                    <strong>{order.shipmentReference || '-'}</strong>
                </div>
            </div>
        )
    }

    if (isCheckingSession) return null

    if (!isLoggedIn) {
        return (
            <div className="login-page">
                <div className="login-container">
                    <img src="/loginlogo.png" alt="Deals Login Logo" className="login-logo" />

                    <h1>Welcome to Deals</h1>
                    <p className="subtitle">Sign in to continue to the admin dashboard.</p>

                    <div className="login-form">
                        <label>Email</label>
                        <input
                            value={email}
                            onChange={(event) => setEmail(event.target.value)}
                            type="email"
                            placeholder="Enter email"
                        />

                        <label>Password</label>
                        <input
                            value={password}
                            onChange={(event) => setPassword(event.target.value)}
                            type="password"
                            placeholder="Enter password"
                        />

                        {loginError && <div className="login-error">{loginError}</div>}

                        <button onClick={handleLogin}>Login</button>

                        <p className="demo-text">
                            Demo credentials: <strong>admin@demo.com / admin123</strong>
                        </p>
                    </div>
                </div>
            </div>
        )
    }

    return (
        <div className="admin-page">
            <header className="admin-hero">
                <div className="hero-left">
                    <img src="/everdeals.png" alt="EverDeals" className="admin-logo" />

                    <div>
                        <p className="eyebrow">EverDeals Admin</p>
                        <h1>{activeView === 'dashboard' ? 'Orders Dashboard' : 'Failed Orders'}</h1>
                        <p>
                            {activeView === 'dashboard'
                                ? 'Monitor orders, filter statuses, and inspect operational details.'
                                : 'Review failed orders and inspect problem cases separately.'}
                        </p>
                    </div>
                </div>

                <div className="hero-actions">
                    <button className="refresh-button" onClick={() => loadOrders(false)}>
                        {isRefreshing ? 'Refreshing...' : 'Refresh Orders'}
                    </button>
                    <button className="logout-button" onClick={handleLogout}>
                        Logout
                    </button>
                </div>
            </header>

            <section className="admin-nav">
                <button
                    className={`admin-nav-button ${activeView === 'dashboard' ? 'active' : ''}`}
                    onClick={() => setActiveView('dashboard')}
                >
                    Dashboard
                </button>
                <button
                    className={`admin-nav-button ${activeView === 'failed' ? 'active' : ''}`}
                    onClick={() => setActiveView('failed')}
                >
                    Failed Orders
                </button>
            </section>

            <section className="summary-grid">
                <article className="summary-card">
                    <span>Total Orders</span>
                    <strong>{summary.total}</strong>
                </article>
                <article className="summary-card">
                    <span>Completed</span>
                    <strong>{summary.completed}</strong>
                </article>
                <article className="summary-card">
                    <span>Pending</span>
                    <strong>{summary.pending}</strong>
                </article>
                <article className="summary-card failed-card">
                    <span>Failed</span>
                    <strong>{summary.failed}</strong>
                </article>
            </section>

            {errorMessage && <div className="info-banner error">{errorMessage}</div>}

            {activeView === 'dashboard' ? (
                <>
                    <section className="toolbar">
                        <div className="filter-group">
                            <label htmlFor="statusFilter">Filter by status</label>
                            <select
                                id="statusFilter"
                                value={statusFilter}
                                onChange={(event) => setStatusFilter(event.target.value)}
                            >
                                {FILTER_OPTIONS.map((status) => (
                                    <option key={status} value={status}>
                                        {status}
                                    </option>
                                ))}
                            </select>

                            <input
                                className="search-input"
                                type="text"
                                placeholder="Search by Order ID"
                                value={searchTerm}
                                onChange={(event) => setSearchTerm(event.target.value)}
                            />
                        </div>
                    </section>

                    <div className="dashboard-layout">
                        <section className="orders-panel">
                            <div className="panel-header">
                                <h2>Orders Table</h2>
                                <span>{filteredOrders.length} visible</span>
                            </div>

                            {isLoading ? (
                                <div className="empty-state">Loading orders...</div>
                            ) : filteredOrders.length === 0 ? (
                                <div className="empty-state">No orders found for this filter.</div>
                            ) : (
                                <div className="table-wrap">
                                    <table className="orders-table">
                                        <thead>
                                        <tr>
                                            <th>Order ID</th>
                                            <th>Customer ID</th>
                                            <th>Status</th>
                                            <th>Created</th>
                                            <th>Shipment Ref</th>
                                        </tr>
                                        </thead>
                                        <tbody>
                                        {filteredOrders.map((order) => (
                                            <tr
                                                key={order.orderId}
                                                className={selectedOrder?.orderId === order.orderId ? 'active-row' : ''}
                                                onClick={() => setSelectedOrder(order)}
                                            >
                                                <td>
                                                    <div className="table-id-cell">
                                                        <strong>{formatShortId(order.orderId)}</strong>
                                                        <span>{order.orderId}</span>
                                                    </div>
                                                </td>
                                                <td>{formatShortId(order.customerId)}</td>
                                                <td>
                            <span className={`status-badge ${getStatusClass(order.status)}`}>
                              {order.status}
                            </span>
                                                </td>
                                                <td>{formatDate(order.createdAt)}</td>
                                                <td>{order.shipmentReference || '-'}</td>
                                            </tr>
                                        ))}
                                        </tbody>
                                    </table>
                                </div>
                            )}
                        </section>

                        <aside className="details-panel">
                            <div className="panel-header">
                                <h2>Order Details</h2>
                            </div>
                            {renderOrderDetails(selectedOrder)}
                        </aside>
                    </div>
                </>
            ) : (
                <>
                    <section className="toolbar">
                        <div className="filter-group">
                            <label htmlFor="failedSearch">Search failed order</label>
                            <input
                                id="failedSearch"
                                className="search-input"
                                type="text"
                                placeholder="Search by Order ID or Customer ID"
                                value={failedSearchTerm}
                                onChange={(event) => setFailedSearchTerm(event.target.value)}
                            />
                        </div>
                    </section>

                    <div className="dashboard-layout">
                        <section className="orders-panel">
                            <div className="panel-header">
                                <h2>Failed Orders</h2>
                                <span>{filteredFailedOrders.length} visible</span>
                            </div>

                            {isLoading ? (
                                <div className="empty-state">Loading orders...</div>
                            ) : filteredFailedOrders.length === 0 ? (
                                <div className="empty-state">No failed orders right now.</div>
                            ) : (
                                <div className="failed-list">
                                    {filteredFailedOrders.map((order) => (
                                        <div
                                            key={order.orderId}
                                            className={`failed-order-card ${
                                                selectedFailedOrder?.orderId === order.orderId ? 'failed-order-card-active' : ''
                                            }`}
                                            onClick={() => setSelectedFailedOrder(order)}
                                        >
                                            <div>
                                                <p>{order.orderId}</p>
                                                <span>{order.customerId}</span>
                                            </div>
                                            <span className="status-badge failed">{order.status}</span>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </section>

                        <aside className="details-panel">
                            <div className="panel-header">
                                <h2>Failed Order Details</h2>
                            </div>
                            {renderOrderDetails(selectedFailedOrder)}
                        </aside>
                    </div>
                </>
            )}
        </div>
    )
}

export default App