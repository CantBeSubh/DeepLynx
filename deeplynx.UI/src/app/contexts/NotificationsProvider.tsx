"use client"

import React, { createContext, useState, useEffect, useContext, ReactNode } from 'react';
import * as signalR from '@microsoft/signalr';

// Define the shape of the notification
interface Notification {
    user: string;
    message: string;
}

// Define the shape of the context value
interface NotificationContextType {
    notifications: Notification[];
}

// Create the context with a default value
const NotificationContext = createContext<NotificationContextType | undefined>(undefined);

// Create the provider component
export const NotificationProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
    const [notifications, setNotifications] = useState<Notification[]>([]);

    useEffect(() => {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("http://localhost:5095/eventNotificationHub")
            .configureLogging(signalR.LogLevel.Information)
            .build();

        connection.on("ReceiveNotification", (user: string, message: string) => {
            setNotifications(prevNotifications => [...prevNotifications, { user, message }]);
        });

        connection.start().catch(err => console.error(err.toString()));

        return () => {
            connection.stop();
        };
    }, []);

    return (
        <NotificationContext.Provider value={{ notifications }}>
            {children}
        </NotificationContext.Provider>
    );
};

// Custom hook to use the NotificationContext
export const useNotifications = (): NotificationContextType => {
    const context = useContext(NotificationContext);
    if (!context) {
        throw new Error("useNotifications must be used within a NotificationProvider");
    }
    return context;
};
