"use client";
import "../globals.css";
import type { AppProps } from "next/app";
import Layout from "./components/LayoutShell";

interface Props {
  params: Promise<{ slug: string[] }>;
  searchParams: Promise<{ sortOrder: string }>;
}

function Home({ params, searchParams }: Props) {
  return <div>DeepLynx HomePage!</div>;
}

export default Home;
