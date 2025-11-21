import React, { useState, useEffect } from 'react';
import { 
  Users, Building2, FileText, TrendingUp, 
  Plus, Edit2, Trash2, Eye, LogOut, Menu, X,
  Search, CheckCircle, Clock, Send
} from 'lucide-react';

// API Configuration - UPDATE THIS TO YOUR BACKEND URL
const API_BASE_URL = 'http://localhost:5000/api';

// Currency symbols
const currencySymbols = {
  INR: '₹',
  USD: '$',
  EUR: '€'
};

const InvoiceManagementApp = () => {
  const [user, setUser] = useState(null);
  const [view, setView] = useState('login');
  const [companies, setCompanies] = useState([]);
  const [clients, setClients] = useState([]);
  const [invoices, setInvoices] = useState([]);
  const [dashboardStats, setDashboardStats] = useState(null);
  const [sidebarOpen, setSidebarOpen] = useState(true);

  // Mock login - Replace with actual API call
  const handleLogin = async (username, password) => {
    // Simulate login
    const mockUser = {
      userId: 1,
      username: username,
      fullName: 'Admin User',
      role: 'Admin',
      email: 'admin@company.com'
    };
    setUser(mockUser);
    setView('dashboard');
    
    // Load mock data
    setCompanies([
      { companyId: 1, companyName: 'Sahir Projects' },
      { companyId: 2, companyName: 'Configurator Solutions' }
    ]);
    
    setClients([
      { clientId: 1, clientName: 'Acme Corp', companyId: 1, currency: 'USD' },
      { clientId: 2, clientName: 'Tech Solutions', companyId: 2, currency: 'EUR' }
    ]);
    
    setInvoices([
      {
        invoiceId: 1,
        invoiceNumber: 'INV-2024-001',
        client: { clientName: 'Acme Corp' },
        company: { companyName: 'Sahir Projects' },
        invoiceDate: '2024-01-15',
        totalAmount: 5000,
        currency: 'USD',
        status: 'Paid'
      },
      {
        invoiceId: 2,
        invoiceNumber: 'INV-2024-002',
        client: { clientName: 'Tech Solutions' },
        company: { companyName: 'Configurator Solutions' },
        invoiceDate: '2024-01-20',
        totalAmount: 3500,
        currency: 'EUR',
        status: 'Sent'
      }
    ]);
    
    setDashboardStats({
      totalInvoices: 2,
      draftInvoices: 0,
      sentInvoices: 1,
      paidInvoices: 1,
      revenueByurrency: [
        { currency: 'USD', total: 5000 },
        { currency: 'EUR', total: 3500 }
      ]
    });
  };

  const handleLogout = () => {
    setUser(null);
    setView('login');
  };

  const canEdit = user && (user.role === 'Admin' || user.role === 'InvoiceCreator');

  if (view === 'login') {
    return <LoginScreen onLogin={handleLogin} />;
  }

  return (
    <div className="flex h-screen bg-gray-50">
      <Sidebar 
        isOpen={sidebarOpen}
        onToggle={() => setSidebarOpen(!sidebarOpen)}
        view={view}
        setView={setView}
        user={user}
        onLogout={handleLogout}
      />

      <div className="flex-1 flex flex-col overflow-hidden">
        <TopBar user={user} companies={companies} />

        <main className="flex-1 overflow-auto p-6">
          {view === 'dashboard' && (
            <Dashboard 
              stats={dashboardStats}
              invoices={invoices}
            />
          )}
          
          {view === 'invoices' && (
            <InvoiceList 
              invoices={invoices}
              canEdit={canEdit}
            />
          )}
          
          {view === 'clients' && (
            <ClientList 
              clients={clients}
              companies={companies}
              canEdit={canEdit}
            />
          )}
        </main>
      </div>
    </div>
  );
};

const LoginScreen = ({ onLogin }) => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');

  const handleSubmit = (e) => {
    e.preventDefault();
    onLogin(username, password);
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-600 to-blue-800 flex items-center justify-center p-4">
      <div className="bg-white rounded-lg shadow-xl p-8 w-full max-w-md">
        <div className="text-center mb-8">
          <h1 className="text-3xl font-bold text-gray-800 mb-2">Invoice Management</h1>
          <p className="text-gray-600">Sign in to your account</p>
        </div>
        
        <form onSubmit={handleSubmit}>
          <div className="mb-4">
            <label className="block text-sm font-medium text-gray-700 mb-2">Username</label>
            <input
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500"
              required
            />
          </div>
          
          <div className="mb-6">
            <label className="block text-sm font-medium text-gray-700 mb-2">Password</label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500"
              required
            />
          </div>
          
          <button
            type="submit"
            className="w-full bg-blue-600 text-white py-2 rounded-lg hover:bg-blue-700 transition"
          >
            Sign In
          </button>
        </form>
        
        <div className="mt-6 text-center text-sm text-gray-600">
          <p>Enter any username and password to demo</p>
        </div>
      </div>
    </div>
  );
};

const Sidebar = ({ isOpen, onToggle, view, setView, user, onLogout }) => {
  const menuItems = [
    { id: 'dashboard', label: 'Dashboard', icon: TrendingUp },
    { id: 'invoices', label: 'Invoices', icon: FileText },
    { id: 'clients', label: 'Clients', icon: Building2 },
  ];

  if (user && user.role === 'Admin') {
    menuItems.push({ id: 'users', label: 'Users', icon: Users });
  }

  return (
    <div className={`bg-gray-800 text-white transition-all duration-300 ${isOpen ? 'w-64' : 'w-20'}`}>
      <div className="p-4 flex justify-between items-center border-b border-gray-700">
        {isOpen && <h2 className="text-xl font-bold">Invoice</h2>}
        <button onClick={onToggle} className="p-2 hover:bg-gray-700 rounded">
          {isOpen ? <X size={20} /> : <Menu size={20} />}
        </button>
      </div>
      
      <nav className="mt-4">
        {menuItems.map(item => {
          const Icon = item.icon;
          return (
            <button
              key={item.id}
              onClick={() => setView(item.id)}
              className={`w-full flex items-center gap-3 px-4 py-3 hover:bg-gray-700 transition ${
                view === item.id ? 'bg-gray-700 border-l-4 border-blue-500' : ''
              }`}
            >
              <Icon size={20} />
              {isOpen && <span>{item.label}</span>}
            </button>
          );
        })}
      </nav>
      
      <div className="absolute bottom-0 w-full border-t border-gray-700">
        <button
          onClick={onLogout}
          className="w-full flex items-center gap-3 px-4 py-3 hover:bg-gray-700"
        >
          <LogOut size={20} />
          {isOpen && <span>Logout</span>}
        </button>
      </div>
    </div>
  );
};

const TopBar = ({ user, companies }) => {
  return (
    <div className="bg-white border-b px-6 py-4 flex justify-between items-center">
      <div className="flex items-center gap-4">
        <select className="px-4 py-2 border rounded-lg">
          <option value="">All Companies</option>
          {companies.map(c => (
            <option key={c.companyId} value={c.companyId}>{c.companyName}</option>
          ))}
        </select>
      </div>
      
      <div className="flex items-center gap-3">
        <div className="text-right">
          <p className="font-medium text-gray-800">{user.fullName}</p>
          <p className="text-sm text-gray-600">{user.role}</p>
        </div>
      </div>
    </div>
  );
};

const Dashboard = ({ stats, invoices }) => {
  if (!stats) return <div>Loading...</div>;

  const recentInvoices = invoices.slice(0, 5);

  return (
    <div>
      <h1 className="text-3xl font-bold text-gray-800 mb-6">Dashboard</h1>
      
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-6">
        <StatCard 
          title="Total Invoices" 
          value={stats.totalInvoices} 
          icon={FileText}
          color="blue"
        />
        <StatCard 
          title="Draft" 
          value={stats.draftInvoices} 
          icon={Clock}
          color="yellow"
        />
        <StatCard 
          title="Sent" 
          value={stats.sentInvoices} 
          icon={Send}
          color="purple"
        />
        <StatCard 
          title="Paid" 
          value={stats.paidInvoices} 
          icon={CheckCircle}
          color="green"
        />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-white rounded-lg shadow p-6">
          <h2 className="text-xl font-bold mb-4">Revenue by Currency</h2>
          {stats.revenueByurrency && stats.revenueByurrency.map(r => (
            <div key={r.currency} className="flex justify-between py-2 border-b">
              <span className="font-medium">{r.currency}</span>
              <span className="text-lg">{currencySymbols[r.currency]}{r.total.toFixed(2)}</span>
            </div>
          ))}
        </div>
        
        <div className="bg-white rounded-lg shadow p-6">
          <h2 className="text-xl font-bold mb-4">Recent Invoices</h2>
          <div className="space-y-2">
            {recentInvoices.map(inv => (
              <div key={inv.invoiceId} className="flex justify-between py-2 border-b">
                <div>
                  <p className="font-medium">{inv.invoiceNumber}</p>
                  <p className="text-sm text-gray-600">{inv.client.clientName}</p>
                </div>
                <div className="text-right">
                  <p className="font-medium">{currencySymbols[inv.currency]}{inv.totalAmount.toFixed(2)}</p>
                  <StatusBadge status={inv.status} />
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};

const StatCard = ({ title, value, icon, color }) => {
  const Icon = icon;
  const colors = {
    blue: 'bg-blue-100 text-blue-600',
    yellow: 'bg-yellow-100 text-yellow-600',
    purple: 'bg-purple-100 text-purple-600',
    green: 'bg-green-100 text-green-600'
  };

  return (
    <div className="bg-white rounded-lg shadow p-6">
      <div className="flex items-center justify-between mb-2">
        <p className="text-gray-600">{title}</p>
        <div className={`p-3 rounded-lg ${colors[color]}`}>
          <Icon size={24} />
        </div>
      </div>
      <p className="text-3xl font-bold">{value}</p>
    </div>
  );
};

const StatusBadge = ({ status }) => {
  const styles = {
    Draft: 'bg-gray-100 text-gray-800',
    Sent: 'bg-blue-100 text-blue-800',
    Paid: 'bg-green-100 text-green-800',
    Cancelled: 'bg-red-100 text-red-800'
  };

  return (
    <span className={`px-2 py-1 rounded-full text-xs font-medium ${styles[status]}`}>
      {status}
    </span>
  );
};

const InvoiceList = ({ invoices, canEdit }) => {
  const [searchTerm, setSearchTerm] = useState('');
  
  const filtered = invoices.filter(inv => 
    inv.invoiceNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
    inv.client.clientName.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold text-gray-800">Invoices</h1>
        {canEdit && (
          <button className="bg-blue-600 text-white px-6 py-2 rounded-lg flex items-center gap-2 hover:bg-blue-700">
            <Plus size={20} />
            New Invoice
          </button>
        )}
      </div>

      <div className="bg-white rounded-lg shadow">
        <div className="p-4 border-b">
          <div className="relative">
            <Search className="absolute left-3 top-3 text-gray-400" size={20} />
            <input
              type="text"
              placeholder="Search invoices..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full pl-10 pr-4 py-2 border rounded-lg"
            />
          </div>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-gray-50 border-b">
              <tr>
                <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">Invoice #</th>
                <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">Client</th>
                <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">Company</th>
                <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">Date</th>
                <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">Amount</th>
                <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">Status</th>
                <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">Actions</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map(inv => (
                <tr key={inv.invoiceId} className="border-b hover:bg-gray-50">
                  <td className="px-6 py-4 font-medium">{inv.invoiceNumber}</td>
                  <td className="px-6 py-4">{inv.client.clientName}</td>
                  <td className="px-6 py-4">{inv.company.companyName}</td>
                  <td className="px-6 py-4">{new Date(inv.invoiceDate).toLocaleDateString()}</td>
                  <td className="px-6 py-4 font-medium">{currencySymbols[inv.currency]}{inv.totalAmount.toFixed(2)}</td>
                  <td className="px-6 py-4"><StatusBadge status={inv.status} /></td>
                  <td className="px-6 py-4">
                    <div className="flex gap-2">
                      <button className="p-1 hover:bg-gray-200 rounded">
                        <Eye size={18} className="text-gray-600" />
                      </button>
                      {canEdit && (
                        <>
                          <button className="p-1 hover:bg-gray-200 rounded">
                            <Edit2 size={18} className="text-blue-600" />
                          </button>
                          <button className="p-1 hover:bg-gray-200 rounded">
                            <Trash2 size={18} className="text-red-600" />
                          </button>
                        </>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

const ClientList = ({ clients, companies, canEdit }) => {
  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold text-gray-800">Clients</h1>
        {canEdit && (
          <button className="bg-blue-600 text-white px-6 py-2 rounded-lg flex items-center gap-2 hover:bg-blue-700">
            <Plus size={20} />
            New Client
          </button>
        )}
      </div>

      <div className="bg-white rounded-lg shadow">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-gray-50 border-b">
              <tr>
                <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">Client Name</th>
                <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">Company</th>
                <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">Currency</th>
                <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">Actions</th>
              </tr>
            </thead>
            <tbody>
              {clients.map(client => {
                const company = companies.find(c => c.companyId === client.companyId);
                return (
                  <tr key={client.clientId} className="border-b hover:bg-gray-50">
                    <td className="px-6 py-4 font-medium">{client.clientName}</td>
                    <td className="px-6 py-4">{company ? company.companyName : ''}</td>
                    <td className="px-6 py-4">{client.currency}</td>
                    <td className="px-6 py-4">
                      <div className="flex gap-2">
                        <button className="p-1 hover:bg-gray-200 rounded">
                          <Eye size={18} className="text-gray-600" />
                        </button>
                        {canEdit && (
                          <>
                            <button className="p-1 hover:bg-gray-200 rounded">
                              <Edit2 size={18} className="text-blue-600" />
                            </button>
                            <button className="p-1 hover:bg-gray-200 rounded">
                              <Trash2 size={18} className="text-red-600" />
                            </button>
                          </>
                        )}
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default InvoiceManagementApp;