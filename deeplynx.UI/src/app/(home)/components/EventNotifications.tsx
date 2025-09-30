"use client"

// components/NotificationList.tsx
import React, { useEffect } from 'react';
import { useNotifications } from '../../contexts/NotificationsProvider';

const EventNotifications = () => {
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

export default EventNotifications;
