// components/NotificationList.tsx
import React, { useEffect, useState } from 'react';
// import { useNotifications } from '../../contexts/NotificationProvider';
import { XMarkIcon, UserCircleIcon } from '@heroicons/react/24/solid';
import { EventNotification } from '../types/types';

// Mock notification data for demo
const mockNotifications: EventNotification[] = [
    {
        id: 1,
        projectId: 'proj-123',
        operation: 'create',
        entityType: 'edge',
        entityId: 'edge-456',
        dataSourceId: 'ds-789',
        dataSourceName: 'Production Database',
        properties: JSON.stringify({
            origin: 'node-1',
            destination: 'node-2'
        }),
        lastUpdatedBy: 'user-123',
        lastUpdatedByName: 'John Doe',
        LastUpdatedAt: new Date()
    }
];

// Mock useNotifications hook for demo
const useNotifications = () => {
    const [notifications, setNotifications] = useState<EventNotification[]>([]);

    useEffect(() => {
        // Simulate receiving a notification after 1 second
        const timer = setTimeout(() => {
            setNotifications(mockNotifications);
        }, 1000);

        return () => clearTimeout(timer);
    }, []);

    return { notifications };
};

export const EventBannerNotification = () => {
    const { notifications } = useNotifications();
    const [visibleNotifications, setVisibleNotifications] = useState<EventNotification[]>([]);

    useEffect(() => {
        console.log('Notifications:', notifications);

        // Add new notifications to visible list
        notifications.forEach(notification => {
            if (!visibleNotifications.find(n => n.id === notification.id)) {
                setVisibleNotifications(prev => [...prev, notification]);

                // Auto-dismiss after 5 seconds
                setTimeout(() => {
                    handleDismiss(notification.id);
                }, 5000);
            }
        });
    }, [notifications]);

    const handleDismiss = (id: number) => {
        setVisibleNotifications(prev => prev.filter(n => n.id !== id));
    };

    const getNotificationMessage = (notification: EventNotification) => {
        return `${notification.lastUpdatedByName} ${notification.operation} an ${notification.entityType} in ${notification.dataSourceName}`;
    };

    return (
        <div className="fixed top-20 right-4 z-50 space-y-2">
            {visibleNotifications.map((notification, index) => (
                <div
                    key={notification.id}
                    className="w-96 bg-white rounded-2xl shadow-2xl overflow-hidden transform transition-all duration-300 ease-out animate-slideIn"
                    style={{
                        animation: `slideIn 0.3s ease-out ${index * 0.1}s both`
                    }}
                >
                    <div className="flex items-start p-4 gap-3">
                        {/* Profile Icon */}
                        <div className="flex-shrink-0">
                            <UserCircleIcon className="w-10 h-10 text-blue-500" />
                        </div>

                        {/* Content */}
                        <div className="flex-1 min-w-0">
                            <div className="flex items-start justify-between gap-2">
                                <div className="flex-1">
                                    <p className="text-sm font-semibold text-gray-900">
                                        {notification.dataSourceName}
                                    </p>
                                    <p className="text-sm text-gray-600 mt-0.5">
                                        {getNotificationMessage(notification)}
                                    </p>
                                </div>

                                {/* Close button */}
                                <button
                                    onClick={() => handleDismiss(notification.id)}
                                    className="flex-shrink-0 text-gray-400 hover:text-gray-600 transition-colors"
                                >
                                    <XMarkIcon className="w-5 h-5" />
                                </button>
                            </div>

                            {/* Timestamp */}
                            <p className="text-xs text-gray-400 mt-1">
                                {notification.LastUpdatedAt?.toLocaleString()}
                            </p>
                        </div>
                    </div>
                </div>
            ))}

            <style>{`
        @keyframes slideIn {
          from {
            transform: translateX(400px);
            opacity: 0;
          }
          to {
            transform: translateX(0);
            opacity: 1;
          }
        }
      `}</style>
        </div>
    );
};

// Test
export const EventNotifications = () => {
    const { notifications } = useNotifications();

    useEffect(() => {
        console.log('Notifications:', notifications);
    }, [notifications]);

    return (
        <div>
            <h2>Notifications</h2>
            <p>Check the console for notifications.</p>
        </div>
    );
};

// import { useNotifications } from "../../contexts/NotificationsProvider"
// import {useState, useEffect} from "react";
//
// const { notifications } = useNotifications();
//
// <div className="dropdown dropdown-end m-1 right-0">
//     <div tabIndex={0} role="button" className="btn btn-ghost m-1">
//         <BellIcon className="size-10" />
//         <span className="px-2 text-white bg-red-700 rounded-full absolute ml-5 mb-10">
//       {notifications.length}
//     </span>
//     </div>
//     <ul
//         tabIndex={0}
//         className="menu dropdown-content bg-base-100 text-base-content rounded-box z-[100]"
//     >
//         {notifications.length > 0 ? (
//             notifications.map((notification, index) => (
//                 <li key={index} className="p-2 bg-base-100">
//                     <div>
//                         <p className="font-bold">{notification.user}</p>
//                         <p>{notification.message}</p>
//                     </div>
//                 </li>
//             ))
//         ) : (
//             <li className="p-2 text-center text-base-content/70">
//                 {/*{t.translations.NO_NOTIFICATIONS}*/}
//             </li>
//         )}
//     </ul>
// </div>