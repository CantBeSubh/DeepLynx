"use client"
import './globals.css';
import type { AppProps } from "next/app";
import Layout from "./components/Layout";

function Home({ Component, pageProps }: AppProps) {
  return (
    <div className="p-4 bg-primary text-black">
      If this text is styled, Tailwind & DaisyUI are working!
    </div>
  );
}

export default Home;