'use client';
import Link from "next/link";

interface Props {
    href: string;
    text: string;
}

const ArrowButton: React.FC<Props> = ({ ...props }: Props) => {
    return (
        <Link key='name' href={props.href}>
            <button className="inline-flex items-center gap-1 pl-2 pr-0.5 py-1 font-bold rounded-2xl bg-inherit text-current hover:text-black hover:bg-[#39526c] dark:border-zinc-700 hover:dark:text-white hover:dark:bg-[#39526c] transition-colors duration-300 ease-in-out group not-prose">
                {props.text}
                <svg viewBox="0 0 24 24" className="fill-none stroke-current stroke-[3px] opacity-50 size-5 group-hover:opacity-100 transition-opacity duration-300 ease-in-out">
                    <line x1="5" y1="12" x2="19" y2="12" className="translate-x-[10px] scale-x-0 group-hover:translate-x-0 group-hover:scale-x-100 transition-transform duration-300 ease-in-out" />
                    <polyline points="12 5 19 12 12 19" className="-translate-x-2 group-hover:translate-x-0 transition-transform duration-300 ease-in-out" />
                </svg>
            </button>
        </Link>
    )
}

export default ArrowButton;