import { useEffect, useMemo, useState } from 'react'
import './App.css'

const API_BASE_URL = 'http://localhost:5258'

function App() {
    const [orders, setOrders] = useState([])
    const [selectedOrder, setSelectedOrder] = useState(null)
    const [statusFilter, setStatusFilter] = useState('All')
    const [isLoading, setIsLoading] = useState(true)
    const [errorMessage, setErrorMessage] = useState('')

    useEffect(() => {
        loadOrders()
    }, [])

    async function loadOrders() {
        setIsLoading(true)
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
        }
    }

    const statusOptions = useMemo(() => {
        const values = new Set(orders.map((order) => order.status))
        return ['All', ...Array.from(values)]
    }, [orders])

    const filteredOrders = useMemo(() => {
        if (statusFilter === 'All') return orders
        return orders.filter((order) => order.status === statusFilter)
    }, [orders, statusFilter])

    const summary = useMemo(() => {
        return {
            total: orders.length,
            completed: orders.filter((order) => order.status === 'Completed').length,
            failed: orders.filter((order) => order.status === 'Failed').length,
            pending: orders.filter((order) => order.status !== 'Completed' && order.status !== 'Failed').length,
        }
    }, [orders])

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

    return (
        <div className="admin-page">
            <header className="admin-hero">
                <div>
                    <p className="eyebrow">EverDeals Admin</p>
                    <h1>Orders Dashboard</h1>
                    <p>Monitor orders, filter statuses, and inspect operational details.</p>
                </div>

                <button className="refresh-button" onClick={loadOrders}>
                    Refresh Orders
                </button>
            </header>

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

            <section className="toolbar">
                <div className="filter-group">
                    <label htmlFor="statusFilter">Filter by status</label>
                    <select
                        id="statusFilter"
                        value={statusFilter}
                        onChange={(event) => setStatusFilter(event.target.value)}
                    >
                        {statusOptions.map((status) => (
                            <option key={status} value={status}>
                                {status}
                            </option>
                        ))}
                    </select>
                </div>
            </section>

            {errorMessage && <div className="info-banner error">{errorMessage}</div>}

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
                                </tr>
                                </thead>
                                <tbody>
                                {filteredOrders.map((order) => (
                                    <tr
                                        key={order.orderId}
                                        className={selectedOrder?.orderId === order.orderId ? 'active-row' : ''}
                                        onClick={() => setSelectedOrder(order)}
                                    >
                                        <td>{order.orderId}</td>
                                        <td>{order.customerId}</td>
                                        <td>
                        <span className={`status-badge ${getStatusClass(order.status)}`}>
                          {order.status}
                        </span>
                                        </td>
                                        <td>{formatDate(order.createdAt)}</td>
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

                    {!selectedOrder ? (
                        <div className="empty-state">Select an order to view details.</div>
                    ) : (
                        <div className="details-grid">
                            <div className="detail-card">
                                <span>Order ID</span>
                                <strong>{selectedOrder.orderId}</strong>
                            </div>
                            <div className="detail-card">
                                <span>Customer ID</span>
                                <strong>{selectedOrder.customerId}</strong>
                            </div>
                            <div className="detail-card">
                                <span>Status</span>
                                <strong>{selectedOrder.status}</strong>
                            </div>
                            <div className="detail-card">
                                <span>Created</span>
                                <strong>{formatDate(selectedOrder.createdAt)}</strong>
                            </div>
                            <div className="detail-card">
                                <span>Inventory Result</span>
                                <strong>{formatDate(selectedOrder.inventoryConfirmedAt)}</strong>
                            </div>
                            <div className="detail-card">
                                <span>Payment Status</span>
                                <strong>{formatDate(selectedOrder.paymentApprovedAt)}</strong>
                            </div>
                            <div className="detail-card">
                                <span>Shipping Status</span>
                                <strong>{formatDate(selectedOrder.shippingCreatedAt)}</strong>
                            </div>
                            <div className="detail-card wide">
                                <span>Shipment Reference</span>
                                <strong>{selectedOrder.shipmentReference || '-'}</strong>
                            </div>
                        </div>
                    )}
                </aside>
            </div>
        </div>
    )
}

export default App
