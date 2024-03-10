create database analytic;

create table users(
    id int primary key generated always as identity,    
    user_id text not null,
    user_name text,    
    CONSTRAINT ux_account_user_id UNIQUE(user_id)
);

-- create table billing_cycles(
--     billing_cycle_id            int primary key generated always as identity,
--     account_id                  int not null,
--     close_at                    timestamptz not null, -- Когда закончится платежный цикл
--     payment_operation_log_id    int, -- Идентификатор операции, завершившей платежный цикл
--     constraint ux_account_close_date UNIQUE(account_id, close_at)
-- )

create table operations(
    operation_id        int primary key generated always as identity,
    operation_date      timestamptz not null,
    public_user_id      text not null,
    debt                numeric(14,4) not null, -- +
    credit              numeric(14,4) not null  -- -
);


select  * 
from    operations

