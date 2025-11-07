"use client";

import React, { useState, useEffect, useCallback } from 'react';
import { useLanguage } from "@/app/contexts/Language";



const OrganizationSettings = ({ }) => {
    const { t } = useLanguage();


    return (
        <div className="p-6 space-y-6">
            <div className="space-y-2">
                <label htmlFor="org-name" className="block text-sm font-medium text-gray-700">
                    Organization Name
                </label>
                <input
                    id="org-name"
                    type="text"
                    className="w-full border border-gray-300 rounded-lg px-4 py-2.5 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition"
                    placeholder="Enter organization name"
                />
            </div>

            <div className="space-y-2">
                <label htmlFor="org-logo" className="block text-sm font-medium text-gray-700">
                    Organization Logo
                </label>
                <div className="flex items-center space-x-4">
                    <div className="w-20 h-20 border-2 border-dashed border-gray-300 rounded-lg flex items-center justify-center bg-gray-50">
                        <svg className="w-8 h-8 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                        </svg>
                    </div>
                    <label htmlFor="org-logo" className="cursor-pointer">
                        <span className="inline-block px-4 py-2 bg-white border border-gray-300 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-50 transition">
                            Choose File
                        </span>
                        <input
                            id="org-logo"
                            type="file"
                            className="hidden"
                            accept="image/*"
                        />
                    </label>
                </div>
                <p className="text-xs text-gray-500">PNG, JPG, GIF up to 10MB</p>
            </div>

            <div className="space-y-2">
                <label htmlFor="storage" className="block text-sm font-medium text-gray-700">
                    Default Object Storage
                </label>
                <input
                    id="storage"
                    type="text"
                    className="w-full border border-gray-300 rounded-lg px-4 py-2.5 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition"
                    placeholder="s3://bucket-name/path"
                />
            </div>
        </div>
    );
};

export default OrganizationSettings;