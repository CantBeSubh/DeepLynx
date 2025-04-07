import React from "react";

const Layout: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  return (
    <div className="flex flex-col min-h-screen">
      {/* Banner/Header */}
      <header className="bg-blue-900 text-white flex justify-between items-center px-6 py-4 shadow-md z-50 fixed w-full">
        <div className="text-xl font-bold">DeepLynx</div>
        <div className="text-sm">User</div>
      </header>

      {/* Page Content */}
      <main className="flex-grow bg-gray-200 pt-16">{children}</main>
    </div>
  );
};

export default Layout;
