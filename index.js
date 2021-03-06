//Main JS file for Discord ACT Bot

//require the discord.js module
const Discord = require('discord.js');
//Create a new discord client
const client = new Discord.Client();

//express here
const express = require('express');
const bodyParser = require('body-parser');
const app = express();
app.use(bodyParser.urlencoded({ extended: true }));

//Load config options
const { prefix, token, express_port, auth_password } = require('./config.json');
//Load User data
const { allowed_userid_arr, act_guid_arr } = require('./userdata.json');

//UUID generation
const { v4: uuidv4 } = require('uuid');
const fs = require('fs');

//When the discord client is ready, run this code
client.once('ready', () => {
	console.log('Discord Client Ready!');
});

//vars for persistent message tracking for parse
let msgObj = {};
let parseFlag = false;

client.on('message', message => {
	//Print message to log (You could uncomment this line if you want full logs)
	//console.log(message.author.username+": "+message.content);
	
	//Check for DM auth request
	if ((message.channel.type === "dm")&&(message.content === `${prefix}auth ${auth_password}`)){
		//Fail if user already exists
		if (allowed_userid_arr.includes(message.author.id)){
			message.channel.send('User already registered.');
		}else {
			let newuuid = uuidv4();
			//Send user info
			message.channel.send(`Your Discord username: ${message.author.username}\nYour Discord ID: ${message.author.id}\nRegistration complete. ACT key: ${newuuid}`);
			//Added user to allowed array
			allowed_userid_arr.push(message.author.id);
			//add uuid to act array
			act_guid_arr.push(newuuid);
			//update users file
			updateUsers();
		}
	}
	
	if (allowed_userid_arr.includes(message.author.id)){
		//Check for allowed userid
		
		//Message author is authorized
		if (message.content === `${prefix}ping`){
			//send back pong in the channel the message was sent
			message.channel.send('Pong');
		/*
		}else if (message.content === `${prefix}user-info`){
			message.channel.send(`Your username: ${message.author.username}\nYour ID: ${message.author.id}`);
		*/
		}else if (message.content === `${prefix}parse`){
			message.channel.send('Ready').then(sentMessage => {
				console.log('Starting parsing');
				msgObj[message.author.id] = sentMessage;
				parseFlag = true;
			});
		}else if (message.content === `${prefix}endparse`){
			console.log('Stopping parsing');
			delete msgObj[message.author.id];
			//Check there are no ongoing parses - this should be done better probably
			if (Object.keys(msgObj).length === 0 && msgObj.constructor === Object){
				parseFlag = false;
			}
		}
	}
});

function updateUsers(){
	//Update the userdata file
	let newdata = {
		'allowed_userid_arr': allowed_userid_arr, 
		'act_guid_arr': act_guid_arr
	};
	let newdatastring = JSON.stringify(newdata);
	fs.writeFileSync('userdata.json', newdatastring);
}

//Login to discord with the app token
client.login(token);

//Express app functionality
app.post('/', (req, res) => {
	//Received a POST call;
	if (act_guid_arr.includes(req.body.guid)){
		//Check if parsing
		if (parseFlag){
			//Received a valid ACT POST call
			//Check discord output has content
			if (req.body.log !== ""){
				//Lookup user by guid from ACT
				msgObj[allowed_userid_arr[act_guid_arr.indexOf(req.body.guid)]].edit(req.body.log);
				console.log(req.body);
				res.send('connected: parsing to discord');
			}else {
				console.log("No log data");
				res.send('Error: missing log');
			}
		}else {
			res.send('connected: not parsing');
		}
	}
});

//Start listening
app.listen(express_port, () =>{
	console.log('app listening on 3000');
});