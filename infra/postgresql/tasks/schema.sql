create DATABASE tasks;
-- 1fb5dac2-221e-4b26-88d1-ee8f62ffee0e -- userid

create table popugs(
    user_id text not null primary key,
    user_name text not null,
    user_role text not null
);

create table tasks(
    task_id int primary key generated always as identity,
    description text not null,
    created_by text not null,
    assign_to text not null,
    is_done boolean not null
);


