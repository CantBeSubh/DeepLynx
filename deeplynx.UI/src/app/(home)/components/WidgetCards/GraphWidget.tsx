import { translations } from "@/app/lib/translations";
import React from "react";

const GraphWidget = () => {
  const locale = "en"; //We could use cookies, context, or router.locale to change language in the future
  const t = translations[locale];
  return (
    <div className="card-body">
      <h2 className="card-title">{t.WidgetCards.GRAPH}</h2>
    </div>
  );
};

export default GraphWidget;
