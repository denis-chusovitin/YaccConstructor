// Signature file for parser generated by fsyacc
module Yard.Frontends.AntlrFrontend.Parser
open Yard.Core.IL
type token = 
  | HENCE
  | DOUBLE_DOT
  | TILDE
  | EXCLAMATION
  | QUESTION
  | SEMICOLON
  | COLON
  | PLUS
  | STAR
  | EQUAL
  | BAR
  | RPAREN
  | LPAREN
  | TERMINAL of (Source.t)
  | LITERAL of (Source.t)
  | IDENTIFIER of (Source.t)
  | T_SCOPE
  | T_FRAGMENT
  | T_OPTIONS
  | T_GRAMMAR
  | EOF
  | ACTION_CODE of (Source.t)
  | ACTION_NAME of (Source.t)
  | CAT_CODE of (Source.t)
  | SINGLELINE_COMMENT of (Source.t)
  | MULTILINE_COMMENT of (Source.t)
type tokenId = 
    | TOKEN_HENCE
    | TOKEN_DOUBLE_DOT
    | TOKEN_TILDE
    | TOKEN_EXCLAMATION
    | TOKEN_QUESTION
    | TOKEN_SEMICOLON
    | TOKEN_COLON
    | TOKEN_PLUS
    | TOKEN_STAR
    | TOKEN_EQUAL
    | TOKEN_BAR
    | TOKEN_RPAREN
    | TOKEN_LPAREN
    | TOKEN_TERMINAL
    | TOKEN_LITERAL
    | TOKEN_IDENTIFIER
    | TOKEN_T_SCOPE
    | TOKEN_T_FRAGMENT
    | TOKEN_T_OPTIONS
    | TOKEN_T_GRAMMAR
    | TOKEN_EOF
    | TOKEN_ACTION_CODE
    | TOKEN_ACTION_NAME
    | TOKEN_CAT_CODE
    | TOKEN_SINGLELINE_COMMENT
    | TOKEN_MULTILINE_COMMENT
    | TOKEN_end_of_input
    | TOKEN_error
type nonTerminalId = 
    | NONTERM__startParseAntlr
    | NONTERM_ParseAntlr
    | NONTERM_GrammarName
    | NONTERM_TopLevelDefs
    | NONTERM_TopLevelDef
    | NONTERM_ActionNameOpt
    | NONTERM_Rule
    | NONTERM_ScopeOpt
    | NONTERM_CatOpt
    | NONTERM_TerminalRule
    | NONTERM_FragmentOpt
    | NONTERM_OptionsOpt
    | NONTERM_RuleBody
    | NONTERM_Alt
    | NONTERM_ActionCodeOpt
    | NONTERM_Seq
    | NONTERM_PredicateOpt
    | NONTERM_Modifier
    | NONTERM_SimpleProduction
    | NONTERM_RuleString
    | NONTERM_RulePart
/// This function maps integers indexes to symbolic token ids
val tagOfToken: token -> int

/// This function maps integers indexes to symbolic token ids
val tokenTagToTokenId: int -> tokenId

/// This function maps production indexes returned in syntax errors to strings representing the non terminal that would be produced by that production
val prodIdxToNonTerminal: int -> nonTerminalId

/// This function gets the name of a token as a string
val token_to_string: token -> string
val ParseAntlr : (Microsoft.FSharp.Text.Lexing.LexBuffer<'cty> -> token) -> Microsoft.FSharp.Text.Lexing.LexBuffer<'cty> -> ((Source.t, Source.t)Grammar.t * (string, string)System.Collections.Generic.Dictionary) 
