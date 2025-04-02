import React from "react";

const Layout: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  return (
    <div className="flex flex-col min-h-screen bg-base-100">
      {/* Navbar */}
      <header className="navbar bg-primary text-white px-6 shadow-lg">
        {/* Left Side - Logo */}
        <div className="flex-1">
          <a className="btn btn-ghost text-2xl font-bold">
            <img src="/logo.svg" alt="DeepLynx" className="h-8 mr-2" />
            DeepLynx
          </a>
        </div>

        {/* Middle - Search Bar */}
        <div className="flex-none">
          <div className="form-control">
            <input
              type="text"
              placeholder="Search"
              className="input input-bordered w-96"
            />
          </div>
        </div>

        {/* Right Side - User Icon */}
        <div className="flex-none">
          <button className="btn btn-circle btn-outline">
            <span className="material-icons">account_circle</span>
          </button>
        </div>
      </header>

      {/* Page Content */}
      <main className="flex-grow p-6">{children}</main>

      {/* Floating Add Button */}
      <button className="btn btn-circle btn-primary fixed bottom-6 right-6 shadow-lg">
        <span className="text-2xl">+</span>
      </button>
    </div>
  );
};

export default Layout;
