USE MASTER
GO

CREATE DATABASE SD4
GO

USE SD4
GO

CREATE TABLE SERVICO (
    Id_servico INT NOT NULL IDENTITY(1,1),   
    Nome_Servico VARCHAR(50) NOT NULL,  
    Descricao VARCHAR(50) NOT NULL,
    PRIMARY KEY (Id_servico)
)

CREATE TABLE TAREFA(
    Id_tarefa INT NOT NULL IDENTITY(1,1),
    Descricao VARCHAR(50) NOT NULL,   
    Id_servico INT NOT NULL,
    Completa BIT NOT NULL DEFAULT 0,  --BIT serve para bools em sql
    PRIMARY KEY (Id_tarefa),
    FOREIGN KEY (Id_servico) REFERENCES SERVICO(Id_servico)       
)

CREATE TABLE CLIENTE (
    Id_cliente INT NOT NULL IDENTITY(1,1),
    Nome_Cliente VARCHAR(50) NOT NULL,    
    Id_servico INT,
    Id_tarefa INT,  
    PRIMARY KEY (Id_cliente),
    FOREIGN KEY (Id_servico) REFERENCES SERVICO(Id_servico),
    FOREIGN KEY (Id_tarefa) REFERENCES TAREFA(Id_tarefa)
)
