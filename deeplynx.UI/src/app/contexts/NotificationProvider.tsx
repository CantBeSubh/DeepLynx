"use client"

import { createContext, useState, useEffect, useContext, ReactNode } from 'react';
import * as signalR from '@microsoft/signalr';
import {EventNotification} from "../(home)/types/types";

// Define the shape of the context value
interface NotificationContextType {
    notifications: EventNotification[];
    isConnected: boolean;
}

// Create the context with a default value
const NotificationContext = createContext<NotificationContextType | undefined>(undefined);

// Create the provider component
export const NotificationProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
    const [notifications, setNotifications] = useState<EventNotification[]>([]);
    const [isConnected, setIsConnected] = useState(false);

    useEffect(() => {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("http://localhost:5095/eventNotificationHub")
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build();

        // Handle receiving notifications
        connection.on("ReceiveNotification", (eventJson: string) => {
            console.log("Raw notification received:", eventJson);
            try {
                const event: EventNotification = JSON.parse(eventJson);
                console.log("Parsed event notification:", event);
                setNotifications(prevNotifications => [...prevNotifications, event]);
            } catch (error) {
                console.error("Error parsing notification:", error);
            }
        });

        // Connection lifecycle handlers
        connection.onreconnecting((error) => {
            console.log("Connection lost, attempting to reconnect...", error);
            setIsConnected(false);
        });

        connection.onreconnected((connectionId) => {
            console.log("Reconnected with connection ID:", connectionId);
            setIsConnected(true);
        });

        connection.onclose((error) => {
            console.log("Connection closed", error);
            setIsConnected(false);
        });

        // Start the connection
        connection.start()
            .then(() => {
                console.log("Connected to SignalR EventNotificationHub");
                setIsConnected(true);
            })
            .catch(err => {
                console.error("SignalR connection error:", err.toString());
                setIsConnected(false);
            });

        return () => {
            connection.stop();
        };
    }, []);

    return (
        <NotificationContext.Provider value={{ notifications, isConnected }}>
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