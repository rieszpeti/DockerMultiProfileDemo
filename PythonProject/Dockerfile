FROM python:latest

WORKDIR /app

COPY PythonProject/requirements.txt /app/requirements.txt
#COPY requirements.txt /app/requirements.txt

RUN pip install --no-cache-dir --upgrade -r /app/requirements.txt
RUN pip install fastapi uvicorn

COPY PythonProject/main.py /app/main.py
#COPY main.py /app/main.py

CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "4949"]
