-- ============================================================
-- RetakePortal — Database Schema for Supabase (PostgreSQL)
-- Run in Supabase → SQL Editor
-- ============================================================

-- Students (imported from SSO once per semester)
CREATE TABLE IF NOT EXISTS students (
    iin              VARCHAR(12)   PRIMARY KEY,
    full_name        VARCHAR(255)  NOT NULL,
    specialty        VARCHAR(255),
    institute        VARCHAR(255),
    department       VARCHAR(255),
    course           INT,
    education_level  VARCHAR(50)   NOT NULL
        CHECK (education_level IN ('bachelor', 'master_sci', 'master_prof', 'doctoral')),
    updated_at       TIMESTAMPTZ   DEFAULT NOW()
);

-- Grades for current semester (imported from SSO, only FX and F)
CREATE TABLE IF NOT EXISTS grades (
    id               SERIAL PRIMARY KEY,
    student_iin      VARCHAR(12)   NOT NULL REFERENCES students(iin) ON DELETE CASCADE,
    discipline_name  VARCHAR(500)  NOT NULL,
    grade            VARCHAR(2)    NOT NULL CHECK (grade IN ('FX', 'F')),
    credits          INT           NOT NULL,
    semester         VARCHAR(20)   NOT NULL,
    UNIQUE (student_iin, discipline_name, semester)
);

-- Specialists (OR and Acts roles)
CREATE TABLE IF NOT EXISTS specialists (
    id            SERIAL PRIMARY KEY,
    username      VARCHAR(100)  NOT NULL UNIQUE,
    password_hash VARCHAR(255)  NOT NULL,
    role          VARCHAR(50)   NOT NULL CHECK (role IN ('or_specialist', 'acts_specialist')),
    full_name     VARCHAR(255)  NOT NULL,
    created_at    TIMESTAMPTZ   DEFAULT NOW()
);

-- Student applications
CREATE TABLE IF NOT EXISTS applications (
    id                 SERIAL PRIMARY KEY,
    iin                VARCHAR(12)     NOT NULL,
    student_full_name  VARCHAR(255)    NOT NULL,
    specialty          VARCHAR(255),
    institute          VARCHAR(255),
    department         VARCHAR(255),
    course             INT,
    education_level    VARCHAR(50)     NOT NULL,
    status             VARCHAR(20)     NOT NULL DEFAULT 'pending'
                           CHECK (status IN ('pending', 'approved', 'rejected')),
    rejection_reason   TEXT,
    total_amount       NUMERIC(12, 2)  NOT NULL DEFAULT 0,
    submitted_at       TIMESTAMPTZ     DEFAULT NOW(),
    reviewed_at        TIMESTAMPTZ,
    reviewed_by        INT             REFERENCES specialists(id)
);

-- Disciplines within an application
CREATE TABLE IF NOT EXISTS application_items (
    id                          SERIAL PRIMARY KEY,
    application_id              INT            NOT NULL REFERENCES applications(id) ON DELETE CASCADE,
    discipline_name             VARCHAR(500)   NOT NULL,
    grade                       VARCHAR(2)     NOT NULL CHECK (grade IN ('FX', 'F')),
    credits                     INT            NOT NULL,
    cost_per_credit             NUMERIC(10, 2) NOT NULL,
    total_cost                  NUMERIC(10, 2) NOT NULL,
    confirmation_document_url   TEXT,
    payment_receipt_url         TEXT
);

-- Expelled students (blocked from retaking)
CREATE TABLE IF NOT EXISTS expelled_students (
    id               SERIAL PRIMARY KEY,
    iin              VARCHAR(12)   NOT NULL,
    discipline_name  VARCHAR(500)  NOT NULL,
    expulsion_date   DATE          NOT NULL,
    act_document_url TEXT,
    added_by         INT           REFERENCES specialists(id),
    added_at         TIMESTAMPTZ   DEFAULT NOW(),
    UNIQUE (iin, discipline_name)
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_students_iin        ON students(iin);
CREATE INDEX IF NOT EXISTS idx_grades_iin          ON grades(student_iin);
CREATE INDEX IF NOT EXISTS idx_grades_semester     ON grades(semester);
CREATE INDEX IF NOT EXISTS idx_applications_iin    ON applications(iin);
CREATE INDEX IF NOT EXISTS idx_applications_status ON applications(status);
CREATE INDEX IF NOT EXISTS idx_expelled_iin        ON expelled_students(iin);

-- ============================================================
-- INITIAL SETUP: navigate to /Admin/Setup to create specialists
-- IMPORT DATA:   navigate to /Admin/Import to load students/grades
-- ============================================================
