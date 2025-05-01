"use client"
import './globals.css';
import type { AppProps } from "next/app";
import Layout from "./components/Layout";

interface Props {
  params: Promise<{ slug: string[] }>;
  searchParams: Promise<{ sortOrder: string }>
}

function Home ({ params, searchParams }: Props) {
  return (
    <div className="p-4">
      DeepLynx HomePage!
    </div>
  );
}

export default Home;