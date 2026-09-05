// src/types/bank.ts
// Gemeinsamer Datenvertrag für die Bank-UI.

export interface BankTransaction {
    Type: 'deposit' | 'withdraw' | 'transfer_sent' | 'transfer_received' | 'system_charge' | string;
    Amount: number;
    TransactionDate: string;
    TargetKontonummer?: string;
    SourceKontonummer?: string;
    Description?: string;
}

/**
 * Fraktionskonto-Daten. TODO(backend): Es gibt aktuell KEINE Serverimplementierung,
 * die diese Felder befüllt. Das Feature ist RAGE:MP-Altbestand, der noch nicht auf
 * die neue Architektur (MyFramework.Core/Infrastructure/FiveM) portiert wurde.
 * Solange `isLeader` nie true wird, bleibt die UI dafür einfach inaktiv.
 */
export interface FactionAccountData {
    bankBalance: number;
    accountNumber: string;
    transactions: BankTransaction[];
}

export interface BankOpenPayload {
    cash: number;
    bankBalance: number;
    accountNumber: string;
    transactions: BankTransaction[];
    /** TODO(backend): wird derzeit nie vom Server gesetzt, siehe FactionAccountData. */
    isLeader?: boolean;
    factionName?: string;
    factionData?: FactionAccountData;
}

export interface BankUpdateBalancesPayload {
    cash: number;
    bankBalance: number;
    /** TODO(backend): wird derzeit nie vom Server gesetzt. */
    factionBankBalance?: number;
}