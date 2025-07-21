"use client";
import Image from "next/image";
import Link from "next/link";
import { useState } from "react";
import ArrowButton from "@/app/(home)/components/ArrowButton";
import { links, LinkT } from "@/app/(home)/(routes)/links";
import "@/app/globals.css";
import { translations } from "@/app/lib/translations";

export default function Signin() {
  const [isChecked, setChecked] = useState(true);

  const locale = "en"; //We could use cookies, context, or router.locale to change language in the future
  const t = translations[locale];

  return (
    <div className="flex flex-col items-center justify-center login min-h-screen gap-4 sm:p-22 font-[family-name:var(--font-roboto-sans)] ">
      <div className="flex flex-col items-center sm:items-start mb-0">
        <Image
          src="/assets/lynx-white.svg"
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
              {t.loginPage.USERNAME}
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
              {t.loginPage.KEEP_SIGNED_IN}
            </label>
            <div className="flex flex-col items-center mt-10">
              <Link
                className="w-70 py-4 mx-5 text-sm text-center text-gray-50 bg-gray-700 border-2 border-black rounded-xl"
                href="/"
              >
                <button className="">{t.loginPage.NEXT}</button>
              </Link>
              <div className="my-15 text-sm text-gray-800 divider divider-primary">
                {t.loginPage.OR}
              </div>

              <Link
                className="w-70 py-4 mx-5 text-sm text-center text-gray-50 bg-gray-700 border-2 border-black rounded-xl"
                href="/api/auth/signin"
              >
                <button className="">{t.loginPage.PIV_CAC_CARD_SIGN_IN}</button>
              </Link>
            </div>
          </div>
        </div>
      </main>
      <Link
        className="text-white hover:bg-[#383838] dark:hover:bg-[#ccc]"
        href="/"
      >
        {" "}
        <u>{t.loginPage.TROUBLE_LOGGING_IN}</u>
      </Link>
      <footer className="flex flex-wrap items-center justify-center gap-8 mt-16 mb-8">
        {links
          .filter(
            (link: LinkT) =>
              link.text.toLowerCase().includes("about") ||
              link.text.toLowerCase().includes("contact")
          )
          .map((link: LinkT, i: number) => (
            <ArrowButton key={i} text={link.text} href={link.href} />
          ))}
      </footer>
    </div>
  );
}
