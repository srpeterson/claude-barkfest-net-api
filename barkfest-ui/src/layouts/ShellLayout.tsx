import { Link, Outlet } from 'react-router-dom'

export function ShellLayout() {
  return (
    <div className="min-h-screen flex flex-col">
      <header className="border-b px-6 py-4 flex items-center gap-6">
        <span className="font-semibold text-lg">Barkfest</span>
        <nav className="flex gap-4 text-sm">
          <Link to="/owners">Owners</Link>
          <Link to="/pets">Pets</Link>
        </nav>
      </header>
      <main className="flex-1 p-6">
        <Outlet />
      </main>
    </div>
  )
}
