"use client";
import Image from "next/image";
import Link from "next/link";
import { useState } from "react";
import ArrowButton from "@/app/(home)/components/ArrowButton";
import { links, LinkT } from "@/app/(home)/(routes)/links";
import "@/app/globals.css";

export default function Login() {
  const [isChecked, setChecked] = useState(true);

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
            <div className="flex flex-col items-center">
              <Image
                src={"/assets/OneID.svg"}
                alt={"OneID logo"}
                width={250}
                height={0}
              />
            </div>

            <div className="flex flex-col items-center mt-2">
              <h2 className="text-sm text-center text-slate-800">
                All data contained within DOE computer systems is owned by DOE
                and may be audited, intercepted, recorded, read, copied, or
                captured in any manner and disclosed in any manner by authorized
                personnel. THERE IS NO RIGHT OF PRIVACY IN THIS SYSTEM. System
                personnel may disclose any potential evidence of crime found on
                DOE computer systems to appropriate authorities. USE OF THIS
                SYSTEM BY ANY USER, AUTHORIZED OR UNAUTHORIZED, CONSTITUTES
                CONSENT TO THIS AUDITING, INTERCEPTION, RECORDING, READING,
                COPYING, CAPTURING, and DISCLOSURE OF COMPUTER ACTIVITY.
                **WARNING**WARNING**WARNING**WARNING**WARNING**
              </h2>
              <h2 className="text-sm text-center text-slate-800">
                Click here for the Vulnerability Disclosure
              </h2>
            </div>
          </div>
        </div>
      </main>
      <Link className="text-white w-full max-w-lg px-40" href="/login/signin">
        <button className="btn btn-outline w-full">Sign In</button>
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
