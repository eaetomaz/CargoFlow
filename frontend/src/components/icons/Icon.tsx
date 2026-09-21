import type { SVGProps } from 'react'

// Ícones de linha minimalistas (mesmo espírito do Feather Icons), sem cor
// própria -- usam sempre currentColor, então herdam a cor do texto ao redor
// (cinza-mudo por padrão, cor de destaque quando ativo/hover). É isso que dá
// o visual "profissional, não colorido" pedido, em vez de emoji.
export type IconProps = SVGProps<SVGSVGElement> & { size?: number }

export function Icon({ size = 18, children, ...props }: IconProps & { children: React.ReactNode }) {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth={1.8}
      strokeLinecap="round"
      strokeLinejoin="round"
      {...props}
    >
      {children}
    </svg>
  )
}
