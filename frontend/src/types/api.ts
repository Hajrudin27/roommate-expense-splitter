export type GroupResponse = {
    id: string;
    name: string;
    currency: string;
    createdAt: string;
  };
  
  export type CreateGroupRequest = {
    name: string;
    currency: string;
  };
  
  export type UserBalanceDto = { userId: string; amount: number };
  export type TransferDto = { fromUserId: string; toUserId: string; amount: number };
  
  export type GetBalancesResponse = {
    groupId: string;
    currency: string;
    netBalances: UserBalanceDto[];
    transfers: TransferDto[];
  };
  
  export type ExpenseShareResponse = { userId: string; amount: number };
  
  export type ExpenseResponse = {
    id: string;
    groupId: string;
    description: string;
    amount: number;
    paidByUserId: string;
    expenseDate: string;
    createdAt: string;
    shares: ExpenseShareResponse[];
  };
  
  export type PaymentResponse = {
    id: string;
    groupId: string;
    fromUserId: string;
    toUserId: string;
    amount: number;
    paymentDate: string;
    createdAt: string;
  };
  
  export type GroupSummaryResponse = {
    group: GroupResponse;
    members: MemberResponse[];
    expenses: ExpenseResponse[];
    payments: PaymentResponse[];
    balances: GetBalancesResponse;
  };
  
  export type CreateExpenseRequest = {
    description: string;
    amount: number;
    paidByUserId: string;
    expenseDate: string; // "YYYY-MM-DD"
    participantUserIds: string[];
  };
  
  export type CreatePaymentRequest = {
    fromUserId: string;
    toUserId: string;
    amount: number;
    paymentDate: string; // "YYYY-MM-DD"
  };
  
  export type MemberResponse = {
    groupId: string;
    userId: string;
    email: string;
    name: string;
    role: string;
    joinedAt: string;
  };
  
  export type CreateMemberRequest = {
    email: string;
    name: string;
    role?: string; // "admin" | "member"
  };
  