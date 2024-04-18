
import { FormLabel, TextField } from '@mui/material';
import './App.css';
import { LocalizationProvider } from '@mui/x-date-pickers';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import { DateTimePicker } from '@mui/x-date-pickers/DateTimePicker';

import { useState } from 'react';

function App() {
  const [date, setDate] = useState(null);
  const [name, setName] = useState("");

  const handleAcceptTime = () => {
    const dateObj = new Date(date).toISOString();
    console.log(dateObj);

    fetch('https://localhost:7168/api/Subscribe',{
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        name,
        date: dateObj
      })
    })
      .then(res => res.json())
      .then(data => console.log(data));

  }
  return (
    <LocalizationProvider dateAdapter={AdapterDayjs}>
      <div className="App">
        <FormLabel style={{display: 'block', marginTop: '100px'}}>Enter event name</FormLabel>
        <TextField className='name-input' style={{margin: '0 0 20px 0'}} value={name} onChange={(newValue) => setName(newValue.target.value)}/>
        <FormLabel style={{display: 'block'}}>Enter event time</FormLabel>
        <DateTimePicker
          className='date-picker'
          views={['year', 'month', 'day', 'hours', 'minutes']}
          value={date}
          onChange={(newValue) => setDate(newValue)}
          onAccept={() => handleAcceptTime()}
        />
      </div>
    </LocalizationProvider>
  );
}

export default App;
