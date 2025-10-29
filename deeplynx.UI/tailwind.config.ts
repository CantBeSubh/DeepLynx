import type { Config } from "tailwindcss";


const config: Config = {
  darkMode: ['class', '[data-theme="dark"]'],
  content: [
    "./src/app/**/*.{js,ts,jsx,tsx}",
    "./src/components/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      spacing: {
        20: "5rem",   // 80px
        24: "6rem",   // 96px
        28: "7rem",   // 112px
        32: "8rem",   // 128px
        // Add more if needed
      },
    },
  },
  plugins: [require("daisyui")],
  // @ts-ignore
  daisyui: {
    themes: ["light", "dark"], // specify which themes you're using
  },
};

export default config;