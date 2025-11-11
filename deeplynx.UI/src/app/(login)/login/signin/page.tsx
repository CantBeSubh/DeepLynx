// src/app/(login)/login/signin/page.tsx

"use client";
import ArrowButton from "@/app/(home)/components/ArrowButton";
import { links, LinkT } from "@/app/(home)/links";
import { useLanguage } from "@/app/contexts/Language";
import "@/app/globals.css";
import Image from "next/image";
import Link from "next/link";
import { useState, useEffect } from "react";
import { useSession, signIn } from "next-auth/react";
import { useRouter } from "next/navigation";

export default function Signin() {
  const [isChecked, setChecked] = useState(true);
  const [isSigningIn, setIsSigningIn] = useState(false);
  const { data: session, status } = useSession();
  const router = useRouter();
  const { t } = useLanguage();

  useEffect(() => {
    // If user is already authenticated, redirect to home
    if (status === "authenticated") {
      router.push("/");
    }
  }, [status, router]);

  // Show loading while checking authentication status
  if (status === "loading") {
    return (
      <div className="flex flex-col items-center justify-center login min-h-screen gap-4 sm:p-22 font-[family-name:var(--font-roboto-sans)]">
        <div className="flex flex-col items-center sm:items-start mb-0">
          <Image
            src="/assets/nexusWhite.png"
            alt="DeepLynx logo"
            width={265.8}
            height={113.9}
            priority
          />
        </div>
        <div className="text-center text-white">
          <div className="loading loading-spinner loading-lg"></div>
          <p className="mt-4">Loading...</p>
        </div>
      </div>
    );
  }

  // If authenticated, don't render login form (redirect will happen)
  if (status === "authenticated") {
    return null;
  }

  const handleOktaSignIn = () => {
    setIsSigningIn(true);
    signIn("okta", { callbackUrl: "/" });
  };

  return (
    <div className="flex flex-col items-center justify-center login min-h-screen gap-4 sm:p-22 font-[family-name:var(--font-roboto-sans)] ">
      <div className="flex flex-col items-center sm:items-start mb-0">
        <Image
          src="/assets/nexusWhite.png"
          alt="DeepLynx logo"
          width={265.8}
          height={113.9}
          priority
        />
      </div>
      <main className="flex flex-col items-center w-full max-w-lg mt-0 mb-2">
        <div className="w-full p-2 bg-white border-2 border-solid rounded-3xl">
          <div className="fieldset m-5">
            <h2 className="text-sm text-center text-slate-800">
              {t.translations.USERNAME}
            </h2>
            <div className="flex flex-col items-center">
              <label className="w-5/6 h-15 mb-2 bg-white border-black input">
                <input type="text" className="text-black" />
              </label>
            </div>
            <label className="text-md text-slate-800 label">
              <input
                type="checkbox"
                defaultChecked={isChecked}
                onChange={(e) => setChecked(e.target.checked)}
                className="checkbox w-6 h-6 appearance-none border-1 border-black rounded-md ml-9"
              />
              {t.translations.KEEP_SIGNED_IN}
            </label>
            <div className="flex flex-col items-center mt-10">
              <Link
                className="w-70 py-4 mx-5 text-sm text-center text-gray-50 bg-gray-700 border-2 border-black rounded-xl"
                href="/"
              >
                <button className="">{t.translations.NEXT}</button>
              </Link>
              <div className="my-15 text-sm text-gray-800 divider divider-primary">
                {t.translations.OR}
              </div>

              <button
                onClick={handleOktaSignIn}
                disabled={isSigningIn}
                className="w-70 py-4 mx-5 text-sm text-center text-gray-50 bg-gray-700 border-2 border-black rounded-xl hover:bg-gray-600 transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
              >
                {isSigningIn && (
                  <span className="loading loading-spinner loading-sm"></span>
                )}
                {t.translations.PIV_CAC_CARD_SIGN_IN}
              </button>
            </div>
          </div>
        </div>
      </main>
      <Link
        className="text-white hover:bg-[#383838] dark:hover:bg-[#ccc]"
        href="/"
      >
        {" "}
        <u>{t.translations.TROUBLE_LOGGING_IN}</u>
      </Link>
      <footer className="flex flex-wrap items-center justify-center gap-8 mt-16 mb-8">
        {/* {links
          .filter(
            (link: LinkT) =>
              link.text.toLowerCase().includes("about") ||
              link.text.toLowerCase().includes("contact")
          )
          .map((link: LinkT, i: number) => (
            <ArrowButton key={i} text={link.text} href={link.href} />
          ))} */}
      </footer>
    </div>
  );
}
