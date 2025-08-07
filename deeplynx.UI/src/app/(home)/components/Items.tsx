import * as React from "react";
import { useMotionValue, Reorder } from "framer-motion";
// import { useRaisedShadow } from "./use-raised-shadow";

interface Props {
  item: string;
}

export const Item = ({ item }: Props) => {
  const y = useMotionValue(0);

  return (
    <Reorder.Item value={item} id={item}>
      <span>{item}</span>
    </Reorder.Item>
  );
};