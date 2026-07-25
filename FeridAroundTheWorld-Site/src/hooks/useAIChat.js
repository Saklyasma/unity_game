import { useCallback, useState } from 'react'
import { useAITutor } from '../context/AIContext.jsx'

/** Thin wrapper around AIContext for the chat input flow: local input value + submit handling. */
export function useAIChat() {
  const { messages, isSending, sendMessage } = useAITutor()
  const [input, setInput] = useState('')

  const submit = useCallback(async () => {
    const text = input.trim()
    if (!text || isSending) return
    setInput('')
    await sendMessage(text)
  }, [input, isSending, sendMessage])

  const submitSuggestion = useCallback((text) => {
    if (isSending) return
    sendMessage(text)
  }, [isSending, sendMessage])

  return { messages, isSending, input, setInput, submit, submitSuggestion }
}
