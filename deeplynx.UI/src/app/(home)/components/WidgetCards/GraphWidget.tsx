import { useLanguage } from "@/app/contexts/Language";

const GraphWidget = () => {
  const { t } = useLanguage();
  return (
    <div className="card-body">
      <h2 className="card-title">{t.translations.GRAPH}</h2>
    </div>
  );
};

export default GraphWidget;
