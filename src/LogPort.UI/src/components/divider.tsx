import React from 'react'
import '../styles/divider.css'

type DividerProps = {
    className?: string
}

export default function Divider({ className = '' }: DividerProps) {
    return <hr className={`divider ${className}`} />
}
