import React, { useEffect, useRef, useState } from "react";

interface SidebarProps {
  isOpen: boolean;
  toggleSidebar: () => void;
}

const Sidebar: React.FC<SidebarProps> = ({ isOpen, toggleSidebar }) => {
  const sidebarRef = useRef<HTMLDivElement>(null);
  const [buttonPosition, setButtonPosition] = useState(0);
  const [selectedFAQ, setSelectedFAQ] = useState<number>(0);

  useEffect(() => {
    if (sidebarRef.current) {
      const sidebarWidth = sidebarRef.current.offsetWidth;
      setButtonPosition(isOpen ? sidebarWidth : 0);
    }
  }, [isOpen]);

  return (
    <div className="relative">
      <div
        ref={sidebarRef}
        className={`fixed top-16 left-0 h-[calc(100%-64px)] bg-base-200 shadow-lg transform ${
          isOpen ? "translate-x-0" : "-translate-x-full"
        } transition-transform duration-300 ease-in-out z-40`}
        style={{ width: isOpen ? "30%" : "0" }}
      >
        {isOpen && (
          <div className="p-10">
            <div className="card-body  p-4">
              <h2 className="card-title text-base-content">
                Welcome to DeepLynx v3!
              </h2>
              <p className="pt-5 text-base-content">
                DeepLynx is a unique datawarehouse designed to provide easy
                collaboration on large projects. DeepLynx allows users to define
                an ontology and then store data under it. Find more information
                on our wiki by clicking here.
              </p>
            </div>
            <h2 className="card-title pb-4 text-base-content">FAQ's</h2>
            <div
              className={`collapse collapse-plus border border-neutral ${
                selectedFAQ === 0 ? "bg-base-200" : "bg-primary"
              }`}
            >
              <input
                type="radio"
                name="faq-accordion"
                checked={selectedFAQ === 0}
                onChange={() => setSelectedFAQ(0)}
              />
              <div className="collapse-title font-semibold text-base-content">
                How do I create an account?
              </div>
              <div className="collapse-content text-base-content text-sm">
                Click the "Sign Up" button in the top right corner and follow
                the registration process.
              </div>
            </div>
            <div
              className={`collapse collapse-plus border border-neutral ${
                selectedFAQ === 1 ? "bg-base-200" : "bg-primary"
              }`}
            >
              <input
                type="radio"
                name="faq-accordion"
                checked={selectedFAQ === 1}
                onChange={() => setSelectedFAQ(1)}
              />
              <div className="collapse-title font-semibold text-base-content">
                I forgot my password. What should I do?
              </div>
              <div className="collapse-content text-sm text-base-content">
                Click on "Forgot Password" on the login page and follow the
                instructions sent to your email.
              </div>
            </div>
            <div
              className={`collapse collapse-plus border border-neutral ${
                selectedFAQ === 2 ? "bg-base-200" : "bg-primary"
              }`}
            >
              <input
                type="radio"
                name="faq-accordion"
                checked={selectedFAQ === 2}
                onChange={() => setSelectedFAQ(2)}
              />
              <div className="collapse-title font-semibold text-base-content">
                How do I update my profile information?
              </div>
              <div className="collapse-content text-sm text-base-content">
                Go to "My Account" settings and select "Edit Profile" to make
                changes.
              </div>
            </div>
          </div>
        )}
      </div>

      <button
        className="fixed top-20 bg-primary text-white px-4 py-2 rounded-r-full shadow-md transition-transform duration-300 ease-in-out z-50"
        style={{ left: `${buttonPosition}px` }}
        onClick={toggleSidebar}
      >
        {isOpen ? "<" : ">"}
      </button>
    </div>
  );
};

export default Sidebar;
