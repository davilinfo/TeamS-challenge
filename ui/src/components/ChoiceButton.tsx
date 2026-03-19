import type { Choice } from "../types/types";
import { ICONS } from "../constants/icons";

interface Props {
  choice: Choice;
  onClick: (id: number) => void;
  disabled?: boolean;
  selected?: boolean;
}

export function ChoiceButton({ choice, onClick, disabled, selected }: Props) {
  return (
    <button
      className={`choice-btn ${selected ? 'choice-btn--selected' : ''}`}
      onClick={() => onClick(choice.id)}
      disabled={disabled}
    >
      <span className="choice-btn__icon">{ICONS[choice.name] ?? '?'}</span>
      <span className="choice-btn__label">{choice.name}</span>
    </button>
  )
}