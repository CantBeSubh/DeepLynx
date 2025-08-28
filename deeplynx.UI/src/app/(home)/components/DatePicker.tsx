"use client";
import { useState } from "react";

export type Query = {
    id: string;
    connector?: string;
    filter?: string;
    operator?: string;
    value?: string; // you can store the combined timestamp here if you want
};
interface DatePickerProps {
    row: Query;
    onChange: (value: string) => void;
}

type DateState = { dateValue?: string };
type TimeState = { timeValue?: string };

export const DatePicker: React.FC<DatePickerProps> = ({ row, onChange }) => {
    const [date, setDate] = useState<DateState>({});
    const [time, setTime] = useState<TimeState>({});

    const handleDateTimeChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const value = e.target.value;
        if (!value) return;
        const [d, t] = value.split("T");
        setDate((r) => ({ ...r, dateValue: d }));
        setTime((r) => ({ ...r, timeValue: t }));
        onChange(e.target.value);
    };

    return (
        <div className="w-full">
            <div className="flex flex-wrap items-center gap-2">
                {/* Date */}
                <div className="relative w-full sm:w-auto">
                    <input
                        type="datetime-local"
                        className="input input-bordered input-sm max-h-8 w-full sm:w-auto"
                        onChange={handleDateTimeChange}
                    />
                </div>
            </div>
        </div>


    );
}
